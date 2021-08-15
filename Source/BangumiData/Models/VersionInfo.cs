using System;

namespace BangumiData.Models
{
    /// <summary>
    /// 版本信息
    /// </summary>
    internal class VersionInfo
    {
        public string Version { get; set; } = string.Empty;
        public bool AutoCheck { get; set; } = false;
        public bool AutoUpdate { get; set; } = false;
        public DateTimeOffset? LastUpdate { get; set; }
        public uint CheckInterval { get; set; } = 7;
        public string[]? SitesEnabledOrder { get; set; }
    }
}
