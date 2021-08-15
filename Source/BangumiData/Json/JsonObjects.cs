using System;
using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace BangumiData.Json
{
    public sealed record RootObject(
        ImmutableDictionary<string, SiteMeta> SiteMeta,
        ImmutableArray<Item> Items)
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonConverters.CustomDateTimeOffsetConverter(),
                new JsonConverters.BroadcastConverter()
            }
        };
    }

    public sealed record SiteMeta(
        string Title,
        string UrlTemplate,
        ImmutableArray<string>? Regions,
        string Type);

    public sealed record Item(
        string Title,
        ImmutableDictionary<string, ImmutableArray<string>> TitleTranslate,
        string Type,
        string Lang,
        string OfficialSite,
        DateTimeOffset? Begin,
        Broadcast? Broadcast,
        DateTimeOffset? End,
        string? Comment,
        ImmutableArray<SiteInfo> Sites);

    public sealed record SiteInfo(
        string Site,
        string? Id,
        string? Url,
        DateTimeOffset? Begin,
        Broadcast? Broadcast,
        string? Comment);
}
