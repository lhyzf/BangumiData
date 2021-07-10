using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BangumiData
{
    /// <summary>
    /// 将 mediaId 映射为 seasonId 的缓存
    /// </summary>
    public class BiliSeasonIdMapper
    {
        private readonly string _filename;
        private Dictionary<string, string> _map;

        public BiliSeasonIdMapper(string filename)
        {
            _filename = filename;
            LoadFromFile().Wait();
        }

        /// <summary>
        /// 获取值，若不在缓存中，则会进行网络请求
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>string.Empty 表示非法数据；当前正在请求或请求失败或接口失效等</returns>
        public async Task<string> GetSeasonIdAsync(string mediaId)
        {
            if (!_map.TryGetValue(mediaId, out string? seasonId))
            {
                _map[mediaId] = string.Empty;
                var url = $"https://bangumi.bilibili.com/view/web_api/media?media_id={mediaId}";
                try
                {
                    var result = await HttpHelper.GetJsonDocumentAsync(url).ConfigureAwait(false);
                    var statusCode = result?.RootElement.GetProperty("code").GetInt32();
                    if (statusCode == 0)
                    {
                        seasonId = result?.RootElement
                            .GetProperty("result")
                            .GetProperty("param")
                            .GetProperty("season_id")
                            .ToString();
                        _map[mediaId] = seasonId ?? throw new ArgumentNullException(nameof(seasonId));
                    }
                    else
                    {
                        seasonId = $"[Error]{statusCode}";
                        _map[mediaId] = seasonId;
                    }
                    await SaveToFile().ConfigureAwait(false);
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine($"网络请求失败：{e}");
                }
                catch (ArgumentNullException e)
                {
                    Debug.WriteLine(e);
                }
                catch (Exception e) when (e is JsonException or KeyNotFoundException)
                {
                    Debug.WriteLine($"Json解析错误：mediaId={mediaId}\n{e}");
                }
                finally
                {
                    if (string.IsNullOrEmpty(seasonId))
                    {
                        _map.Remove(mediaId);
                        await SaveToFile().ConfigureAwait(false);
                    }
                }
                return (seasonId?.StartsWith("[Error]") ?? true) ? string.Empty : seasonId;
            }
            else
            {
                if (seasonId.StartsWith("[Error]"))
                {
                    var code = seasonId.Substring("[Error]".Length);
                    if (code == "-404")
                    {
                        return string.Empty;
                    }
                    else
                    {
                        _map.Remove(mediaId);
                        return await GetSeasonIdAsync(mediaId);
                    }
                }
            }
            return seasonId;
        }

        private async Task LoadFromFile()
        {
            _map = await FileHelper.ReadAsync<Dictionary<string, string>>(_filename).ConfigureAwait(false) ?? new Dictionary<string, string>();
        }

        public async Task SaveToFile()
        {
            await FileHelper.WriteAsync(_filename, _map).ConfigureAwait(false);
        }
    }
}
