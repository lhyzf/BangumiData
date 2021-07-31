using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BangumiData.Models;

namespace BangumiData
{
    public class BangumiDataApi : BangumiDataBaseApi
    {
        private const string BangumiDataUrl = "https://api.github.com/repos/bangumi-data/bangumi-data/tags";
        private const string BangumiDataCDNUrl = "https://cdn.jsdelivr.net/npm/bangumi-data@0.3/dist/data.json";

        /// <summary>
        /// 文件夹路径
        /// </summary>
        private string _directory;
        private string _data_json;
        private string _map_json;
        private string _config_json;

        // 配置文件信息
        private VersionInfo _vInfo;
        public string Version => _vInfo.Version;
        public DateTimeOffset? LastUpdate => _vInfo.LastUpdate;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Require read/write folder path to save bangumi-data cache file.</param>
        private BangumiDataApi(string directory)
        {
            SetDirectory(directory);
        }

        public BangumiDataApi(string directory,
            EventHandler newVersionFoundedEventHandler = null,
            EventHandler downloadStartedEventHandler = null,
            EventHandler versionUpdatedEventHandler = null) : this(directory)
        {
            LoadConfig().Wait();
            LoadData().Wait();
            NewVersionFounded += newVersionFoundedEventHandler;
            DownloadStarted += downloadStartedEventHandler;
            VersionUpdated += versionUpdatedEventHandler;
            _ = CheckUpdate();
        }


        /// <summary>
        /// 根据网站放送开始时间推测更新时间
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dateTime">放送日期</param>
        /// <returns></returns>
        public DateTimeOffset? GetAirTimeByBangumiId(string id, DateTimeOffset dateTime)
        {
            var siteList = GetOrderedSitesByBangumiId(id);
            foreach (var site in siteList)
            {
                if (site.Broadcast != null)
                {
                    return site.Broadcast.Round(dateTime);
                }
                else if (site.Begin is DateTimeOffset begin)
                {
                    var duration = dateTime - begin.Date;
                    var count = (int)Math.Round(duration.Days / 7.0, MidpointRounding.AwayFromZero);
                    return begin.AddDays(7 * count);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据Bangumi的ID返回启用的排好序的放送网站
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<Site>> GetAirSitesByBangumiId(string id)
        {
            var siteList = GetOrderedSitesByBangumiId(id);
            foreach (var site in siteList)
            {
                if (!string.IsNullOrEmpty(site.Id))
                {
                    site.Url = Root.SiteMeta[site.SiteName].UrlTemplate.Replace("{{id}}", site.Id);
                }
            }
            // 启用设置，将 mediaid 转换为 seasonid
            if (_mapper != null)
            {
                var biliSite = siteList.FirstOrDefault(s => s.SiteName.StartsWith("bilibili"));
                if (biliSite?.Id != null)
                {
                    var seasonId = await _mapper.GetSeasonIdAsync(biliSite.Id).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(seasonId))
                    {
                        biliSite.Url = "bilibili://bangumi/season/" + seasonId;
                    }
                }
            }
            foreach (var site in siteList)
            {
                site.SiteName = Root.SiteMeta[site.SiteName].Title;
            }
            return siteList.ToList();
        }

        /// <summary>
        /// 根据Bangumi的ID返回所有放送网站
        /// </summary>
        private IEnumerable<Site> GetOrderedSitesByBangumiId(string id)
        {
            var bangumiItem = GetItemById(id);
            if (bangumiItem?.Sites == null || _vInfo.SitesEnabledOrder == null)
            {
                yield break;
            }
            var sites = bangumiItem.Sites;
            foreach (var item in _vInfo.SitesEnabledOrder)
            {
                // 未标明的资源站使用番剧标题作为ID
                if (Root.SiteMeta[item].Type == "resource" && !sites.Any(it => it.SiteName == item))
                {
                    yield return new Site
                    {
                        SiteName = item,
                        Id = bangumiItem.TitleTranslate.FirstOrDefault().Value?.FirstOrDefault() ??
                             bangumiItem.Title
                    };
                }
                var site = sites.FirstOrDefault(it => it.SiteName == item);
                if (site == null)
                {
                    continue;
                }
                yield return site.Clone();
            }
        }



        #region Bili App
        /// <summary>
        /// 哔哩哔哩 seasonId 转换映射
        /// </summary>
        public BiliSeasonIdMapper? _mapper;

        /// <summary>
        /// 使用 哔哩哔哩动画 应用打开时需要对 id 进行转换
        /// </summary>
        public bool UseBiliApp
        {
            get => _mapper != null;
            set
            {
                if (value)
                {
                    _mapper = new BiliSeasonIdMapper(_map_json);
                }
                else
                {
                    _mapper = null;
                }
            }
        }
        #endregion

        #region Update
        public bool AutoCheck
        {
            get => _vInfo.AutoCheck;
            set
            {
                if (_vInfo.AutoCheck != value)
                {
                    _vInfo.AutoCheck = value;
                    // 关闭自动检查更新后恢复默认设置
                    if (!value)
                    {
                        _vInfo.AutoUpdate = false;
                        _vInfo.CheckInterval = 7;
                    }
                    SaveConfig().Wait();
                }
            }
        }

        public bool AutoUpdate
        {
            get => _vInfo.AutoUpdate;
            set
            {
                if (_vInfo.AutoUpdate != value)
                {
                    _vInfo.AutoUpdate = value;
                    SaveConfig().Wait();
                }
            }
        }

        public uint CheckInterval
        {
            get => _vInfo.CheckInterval;
            set
            {
                if (_vInfo.CheckInterval != value)
                {
                    _vInfo.CheckInterval = value;
                    SaveConfig().Wait();
                }
            }
        }

        public event EventHandler NewVersionFounded;
        public event EventHandler DownloadStarted;
        public event EventHandler VersionUpdated;

        /// <summary>
        /// 尝试获取最新版本号
        /// </summary>
        /// <returns>返回最新版本号，失败后返回空字符串</returns>
        public async Task<(bool IsSuccess, string Version)> TryGetLatestVersion()
        {
            try
            {
                var result = await HttpHelper.GetJsonDocumentAsync(BangumiDataUrl).ConfigureAwait(false);
                var version = result.RootElement[0].GetProperty("name").GetString();
                if (string.IsNullOrEmpty(version))
                {
                    return (false, string.Empty);
                }
                return (true, version);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// 获取最新版本并下载数据
        /// </summary>
        /// <param name="newVersion">若已获取版本号，可提供参数以跳过该方法的版本号检查</param>
        /// <returns>当前已是最新版本或更新版本成功时返回 true</returns>
        public async Task<bool> DownloadLatestVersion(string newVersion = "")
        {
            // 未提供版本参数，在此处获取版本
            if (string.IsNullOrEmpty(newVersion))
            {
                var latestVersion = await TryGetLatestVersion().ConfigureAwait(false);
                // 获取最新版本失败
                if (!latestVersion.IsSuccess)
                {
                    return false;
                }
                // 已是最新版本
                if (Version == latestVersion.Version)
                {
                    _vInfo.LastUpdate = DateTimeOffset.UtcNow;
                    await SaveConfig().ConfigureAwait(false);
                    return true;
                }
                newVersion = latestVersion.Version;
            }
            else
            {
                if (newVersion == _vInfo.Version)
                {
                    return true;
                }
            }
            RaiseDownloadStartedEvent();
            try
            {
                // 下载并保存数据
                var data = await HttpHelper.GetStringAsync(BangumiDataCDNUrl).ConfigureAwait(false);
                var root = JsonSerializer.Deserialize<RootObject>(data, RootObject.GetJsonSerializerOptions());
                if (root == null)
                {
                    throw new ArgumentNullException(nameof(root));
                }
                Init(root);
                await SaveData().ConfigureAwait(false);
                _vInfo.Version = newVersion;
                _vInfo.LastUpdate = DateTimeOffset.UtcNow;
                // 未设置站点时，设置默认值
                if (_vInfo.SitesEnabledOrder == null)
                {
                    _vInfo.SitesEnabledOrder = Root.SiteMeta.Where(it => it.Value.Type == "onair").Select(it => it.Key).ToArray();
                }
                // 检查更新后是否有网站减少
                var sitesEnabledOrder = _vInfo.SitesEnabledOrder.ToList();
                var changed = false;
                for (int i = sitesEnabledOrder.Count - 1; i >= 0; i--)
                {
                    if (!Root.SiteMeta.TryGetValue(sitesEnabledOrder[i], out _))
                    {
                        sitesEnabledOrder.RemoveAt(i);
                        changed = true;
                    }
                }
                if (changed)
                {
                    _vInfo.SitesEnabledOrder = sitesEnabledOrder.ToArray();
                }
                await SaveConfig().ConfigureAwait(false);
                RaiseVersionUpdatedEvent();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private async Task CheckUpdate()
        {
            // 自动检查更新
            if (AutoCheck && DateTimeOffset.UtcNow.Date >= LastUpdate?.Date.AddDays(CheckInterval))
            {
                if (AutoUpdate)
                {
                    await DownloadLatestVersion();
                }
                else
                {
                    var latestVersion = await TryGetLatestVersion().ConfigureAwait(false);
                    // 获取最新版本失败
                    if (latestVersion.IsSuccess && Version != latestVersion.Version)
                    {
                        RaiseNewVersionFoundedEvent();
                    }
                }
            }
        }

        private void RaiseNewVersionFoundedEvent()
        {
            var e = NewVersionFounded;
            e?.Invoke(this, new EventArgs());
        }
        private void RaiseDownloadStartedEvent()
        {
            var e = DownloadStarted;
            e?.Invoke(this, new EventArgs());
        }
        private void RaiseVersionUpdatedEvent()
        {
            var e = VersionUpdated;
            e?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Sites Order
        /// <summary>
        /// 获取未启用的站点
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, SiteMeta> GetDisabledSites()
        {
            var sites = new Dictionary<string, SiteMeta>();
            if (Root.SiteMeta != null && _vInfo.SitesEnabledOrder != null)
            {
                foreach (var item in Root.SiteMeta)
                {
                    if (item.Key != "bangumi" && !_vInfo.SitesEnabledOrder.Contains(item.Key))
                    {
                        sites.Add(item.Key, item.Value);
                    }
                }
            }
            return sites;
        }

        /// <summary>
        /// 获取已启用站点以及顺序
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, SiteMeta> GetEnabledSites()
        {
            var sites = new Dictionary<string, SiteMeta>();
            if (Root.SiteMeta != null && _vInfo.SitesEnabledOrder != null)
            {
                foreach (var site in _vInfo.SitesEnabledOrder)
                {
                    if (Root.SiteMeta.TryGetValue(site, out var siteMeta))
                    {
                        sites.Add(site, siteMeta);
                    }
                }
            }
            return sites;
        }

        /// <summary>
        /// 设置启用的站点以及顺序
        /// </summary>
        /// <param name="siteKeys"></param>
        public async Task SetSitesEnabledOrder(string[] siteKeys)
        {
            _vInfo.SitesEnabledOrder = siteKeys;
            await SaveConfig().ConfigureAwait(false);
        }
        #endregion

        #region File
        private void SetDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _directory = directory;
            _data_json = Path.Combine(_directory, "data.json");
            _map_json = Path.Combine(_directory, "map.json");
            _config_json = Path.Combine(_directory, "config.json");
        }

        private async Task LoadData()
        {
            RootObject? root = null;
            if (File.Exists(_data_json))
            {
                root = await FileHelper.ReadAsync<RootObject>(_data_json, RootObject.GetJsonSerializerOptions()).ConfigureAwait(false) ?? new RootObject();
                // 未设置站点时，设置默认值
                if (_vInfo.SitesEnabledOrder == null)
                {
                    await SetSitesEnabledOrder(root.SiteMeta.Where(it => it.Value.Type == "onair").Select(it => it.Key).ToArray()).ConfigureAwait(false);
                }
            }
            Init(root ?? new RootObject());
        }

        private async Task SaveData()
        {
            await FileHelper.WriteAsync(_data_json, Root, RootObject.GetJsonSerializerOptions()).ConfigureAwait(false);
        }

        private async Task LoadConfig()
        {
            _vInfo = await FileHelper.ReadAsync<VersionInfo>(_config_json).ConfigureAwait(false) ?? new VersionInfo();
        }

        private async Task SaveConfig()
        {
            await FileHelper.WriteAsync(_config_json, _vInfo).ConfigureAwait(false);
        }
        #endregion
    }
}
