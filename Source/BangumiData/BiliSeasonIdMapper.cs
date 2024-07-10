using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BangumiData.Interfaces;

namespace BangumiData
{
    /// <summary>
    /// 将 mediaId 映射为 seasonId 的缓存
    /// </summary>
    public class BiliSeasonIdMapper : ConcurrentDictionary<string, string>
    {
        private const string PersistenceKey = "map";
        private const string ErrorPrefix = "[Error]";
        private readonly IPersistence? _persistence;

        public BiliSeasonIdMapper(IPersistence? persistence)
            : base(persistence?.Read<ConcurrentDictionary<string, string>>(PersistenceKey) ?? new ConcurrentDictionary<string, string>())
        {
            _persistence = persistence;
        }

        /// <summary>
        /// 获取值，若不在缓存中，则会进行网络请求
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns>string.Empty 表示非法数据；当前正在请求或请求失败或接口失效等</returns>
        public async Task<string> GetSeasonIdAsync(string mediaId)
        {
            if (!TryGetValue(mediaId, out string? seasonId))
            {
                this[mediaId] = string.Empty;
                var url = $"https://api.bilibili.com/pgc/review/user?media_id={mediaId}";
                try
                {
                    var result = await HttpHelper.GetJsonDocumentAsync(url).ConfigureAwait(false);
                    var statusCode = result?.RootElement.GetProperty("code").GetInt32();
                    if (statusCode == 0)
                    {
                        seasonId = result?.RootElement
                            .GetProperty("result")
                            .GetProperty("media")
                            .GetProperty("season_id")
                            .ToString();
                        this[mediaId] = seasonId ?? throw new ArgumentNullException(nameof(seasonId));
                    }
                    else
                    {
                        seasonId = $"{ErrorPrefix}{statusCode}";
                        this[mediaId] = seasonId;
                    }
                    _persistence?.Save(PersistenceKey, this);
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
                        TryRemove(mediaId, out _);
                    }
                }
                return (seasonId?.StartsWith(ErrorPrefix) ?? true) ? string.Empty : seasonId;
            }
            else if (seasonId.StartsWith(ErrorPrefix))
            {
                var code = seasonId.Substring(ErrorPrefix.Length);
                if (code == "-404")
                {
                    return string.Empty;
                }
                else
                {
                    TryRemove(mediaId, out _);
                    return await GetSeasonIdAsync(mediaId);
                }
            }
            return seasonId;
        }
    }
}
