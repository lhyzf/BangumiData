#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BangumiData.Models
{
    public class RootObject
    {

        /// <summary>
        /// Examples: {"bangumi":{"title":"番组计划","urlTemplate":"http://bangumi.tv/subject/{{id}}","type":"info"},"saraba1st":{"title":"Stage1st","urlTemplate":"https://bbs.saraba1st.com/2b/thread-{{id}}-1-1.html","type":"info"},"acfun":{"title":"AcFun","urlTemplate":"http://www.acfun.cn/v/ab{{id}}","type":"onair"},"bilibili":{"title":"哔哩哔哩","urlTemplate":"https://bangumi.bilibili.com/anime/{{id}}","type":"onair"},"tucao":{"title":"TUCAO","urlTemplate":"http://www.tucao.tv/index.php?m=search&c=index&a=init2&q={{id}}","type":"onair"},"sohu":{"title":"搜狐视频","urlTemplate":"https://tv.sohu.com/{{id}}","type":"onair"},"youku":{"title":"优酷","urlTemplate":"https://list.youku.com/show/id_z{{id}}.html","type":"onair"},"tudou":{"title":"土豆","urlTemplate":"https://www.tudou.com/albumcover/{{id}}.html","type":"onair"},"qq":{"title":"腾讯视频","urlTemplate":"https://v.qq.com/detail/{{id}}.html","type":"onair"},"iqiyi":{"title":"爱奇艺","urlTemplate":"https://www.iqiyi.com/{{id}}.html","type":"onair"},"letv":{"title":"乐视","urlTemplate":"https://www.le.com/comic/{{id}}.html","type":"onair"},"pptv":{"title":"PPTV","urlTemplate":"http://v.pptv.com/page/{{id}}.html","type":"onair"},"kankan":{"title":"响巢看看","urlTemplate":"http://movie.kankan.com/movie/{{id}}","type":"onair"},"mgtv":{"title":"芒果tv","urlTemplate":"https://www.mgtv.com/h/{{id}}.html","type":"onair"},"nicovideo":{"title":"Niconico","urlTemplate":"https://ch.nicovideo.jp/{{id}}","type":"onair"},"netflix":{"title":"Netflix","urlTemplate":"https://www.netflix.com/title/{{id}}","type":"onair"},"dmhy":{"title":"动漫花园","urlTemplate":"https://share.dmhy.org/topics/list?keyword={{id}}","type":"resource"},"nyaa":{"title":"nyaa","urlTemplate":"https://www.nyaa.se/?page=search&term={{id}}","type":"resource"}}
        /// </summary>
        [JsonPropertyName("siteMeta")]
        public Dictionary<string, SiteMeta> SiteMeta { get; set; }

        /// <summary>
        /// Examples: [{"title":"新しい動画 3つのはなし","titleTranslate":{"zh-Hans":["新动画 三个故事"],"en":["Three Tales"]},"type":"tv","lang":"ja","officialSite":"","begin":"1960-01-15T16:00:00Z","end":"1960-01-15T16:30:00Z","comment":"","sites":[{"site":"bangumi","id":"213759"}]},{"title":"ゲゲゲの鬼太郎","titleTranslate":{"zh-Hans":["鬼太郎","咯咯咯鬼太郎 1"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-01-03T16:00:00Z","end":"1969-03-30T16:30:00Z","comment":"","sites":[{"site":"iqiyi","id":"a_19rrhb12tt","begin":"2015-10-29T08:31:41Z","official":true,"premuiumOnly":false,"censored":null,"exist":true,"comment":""},{"site":"bangumi","id":"40379"}]},{"title":"巨人の星","titleTranslate":{"zh-Hans":["巨人之星"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-03-30T16:00:00Z","end":"1971-09-18T16:30:00Z","comment":"","sites":[{"site":"bangumi","id":"41983"},{"site":"nicovideo","id":"kyojin-no-hoshi","begin":"2012-12-24T06:00:00Z","official":true,"premuiumOnly":true,"censored":null,"exist":true,"comment":""}]},{"title":"ゲゲゲの鬼太郎","titleTranslate":{"zh-Hans":["鬼太郎"]},"type":"movie","lang":"ja","officialSite":"","begin":"1968-07-21T16:00:00Z","end":"1968-07-21T17:00:00Z","comment":"","sites":[{"site":"bangumi","id":"211091"}]},{"title":"サスケ","titleTranslate":{"zh-Hans":["死神少年","佐助"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-09-03T13:15:00Z","end":"1969-03-25T13:45:00Z","comment":"","sites":[{"site":"bangumi","id":"150325"}]},{"title":"妖怪人間ベム","titleTranslate":{"zh-Hans":["妖怪人间贝姆"]},"type":"tv","lang":"ja","officialSite":"","begin":"1968-10-07T07:00:00Z","end":"1969-03-31T07:24:00Z","comment":"","sites":[{"site":"bangumi","id":"53743"}]}]
        /// </summary>
        [JsonPropertyName("items")]
        public Item[] Items { get; set; }
    }

    public class SiteMeta
    {

        /// <summary>
        /// Examples: "番组计划",AcFun","哔哩哔哩","动漫花园"
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Examples: "http://bangumi.tv/subject/{{id}}","http://www.acfun.cn/v/ab{{id}}",
        /// "https://bangumi.bilibili.com/anime/{{id}}","https://share.dmhy.org/topics/list?keyword={{id}}"
        /// </summary>
        [JsonPropertyName("urlTemplate")]
        public string UrlTemplate { get; set; }

        /// <summary>
        /// Examples: ["CN"], ["CN","JP","TW","HK","MO"]
        /// </summary>
        [JsonPropertyName("regions")]
        public string[]? Regions { get; set; }

        /// <summary>
        /// Examples: "resource","info","onair"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Item
    {

        /// <summary>
        /// Examples: "新しい動画 3つのはなし", "ゲゲゲの鬼太郎", "巨人の星", "サスケ", "妖怪人間ベム"
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Examples: {"zh-Hans":["新动画 三个故事"],"en":["Three Tales"]}, {"zh-Hans":["鬼太郎","咯咯咯鬼太郎 1"]}, {"zh-Hans":["巨人之星"]}, {"zh-Hans":["鬼太郎"]}, {"zh-Hans":["死神少年","佐助"]}
        /// <br/>Keys in <see cref="Lang"/>
        /// </summary>
        [JsonPropertyName("titleTranslate")]
        public Dictionary<string, string[]> TitleTranslate { get; set; }

        /// <summary>
        /// Examples: "tv", "web", "movie", "ova"
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Examples: "ja", "en", "zh-Hans", "zh-Hant"
        /// </summary>
        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        /// <summary>
        /// Examples: "", "http://www.tatsunoko.co.jp/works/archive/kurenai.html", "http://www.fujitv.co.jp/sazaesan/", "http://www.toei-anim.co.jp/lineup/tv/tigermask/", "http://www.tatsunoko.co.jp/tatsunoko_gekijo/"
        /// </summary>
        [JsonPropertyName("officialSite")]
        public string OfficialSite { get; set; }

        /// <summary>
        /// Examples: "1960-01-15T16:00:00Z", "1968-01-03T16:00:00Z", "1968-03-30T16:00:00Z", "1968-07-21T16:00:00Z", "1968-09-03T13:15:00Z"
        /// </summary>
        [JsonPropertyName("begin")]
        public DateTimeOffset? Begin { get; set; }

        /// <summary>
        /// Examples: <br/>
        /// 一次性："R/2020-01-01T13:00:00Z/P0D", <br/>
        /// 周播："R/2020-01-01T13:00:00Z/P7D", <br/>
        /// 日播："R/2020-01-01T13:00:00Z/P1D", <br/>
        /// 月播："R/2020-01-01T13:00:00Z/P1M"
        /// </summary>
        [JsonPropertyName("broadcast")]
        public Broadcast? Broadcast { get; set; }

        /// <summary>
        /// Examples: "1960-01-15T16:30:00Z", "1969-03-30T16:30:00Z", "1971-09-18T16:30:00Z", "1968-07-21T17:00:00Z", "1969-03-25T13:45:00Z"
        /// </summary>
        [JsonPropertyName("end")]
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// Examples: ""
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// Examples: [{"site":"bangumi","id":"213759"}], [{"site":"iqiyi","id":"a_19rrhb12tt","begin":"2015-10-29T08:31:41Z","official":true,"premuiumOnly":false,"censored":null,"exist":true,"comment":""},{"site":"bangumi","id":"40379"}], [{"site":"bangumi","id":"41983"},{"site":"nicovideo","id":"kyojin-no-hoshi","begin":"2012-12-24T06:00:00Z","official":true,"premuiumOnly":true,"censored":null,"exist":true,"comment":""}], [{"site":"bangumi","id":"211091"}], [{"site":"bangumi","id":"150325"}]
        /// </summary>
        [JsonPropertyName("sites")]
        public Site[] Sites { get; set; }
    }

    public class Site
    {

        /// <summary>
        /// Examples: "bangumi", "iqiyi", "nicovideo", "bilibili", "pptv"
        /// </summary>
        [JsonPropertyName("site")]
        public string SiteName { get; set; }

        /// <summary>
        /// Examples: "213759", "a_19rrhb12tt", "40379", "41983", "kyojin-no-hoshi"
        /// <br/>If <see cref="Id"/> is <see langword="null"/>, check <see cref="Url"/>.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Examples: "http://www.iqiyi.com/dongman/20130715/839f4bf2c2cbe419.html", "http://www.iqiyi.com/dongman/20130715/d7a321d48f825733.html", "http://www.iqiyi.com/v_19rrifx03h.html", "http://www.iqiyi.com/v_19rrifx03f.html", "http://www.iqiyi.com/dongman/20130715/75c20dc1fe211093.html"
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// Examples: "2015-10-29T08:31:41Z", "2012-12-24T06:00:00Z", "1969-10-04T16:00:00Z", "1969-10-04T16:00:01Z", "2016-12-05T06:24:50Z"
        /// </summary>
        [JsonPropertyName("begin")]
        public DateTimeOffset? Begin { get; set; }

        /// <summary>
        /// Examples: <br/>
        /// 一次性："R/2020-01-01T13:00:00Z/P0D", <br/>
        /// 周播："R/2020-01-01T13:00:00Z/P7D", <br/>
        /// 日播："R/2020-01-01T13:00:00Z/P1D", <br/>
        /// 月播："R/2020-01-01T13:00:00Z/P1M"
        /// </summary>
        [JsonPropertyName("broadcast")]
        public Broadcast? Broadcast { get; set; }

        /// <summary>
        /// Examples: "", "港区可见", "分割为 TV 放送", "原始版本为剧场版但是 Netflix 目前将其做成 14 集 TV 分割形式放送"
        /// </summary>
        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        public Site Clone() => new Site()
        {
            SiteName = SiteName,
            Id = Id,
            Begin = Begin,
            Broadcast = Broadcast,
            Url = Url,
            Comment = Comment
        };
    }
}
