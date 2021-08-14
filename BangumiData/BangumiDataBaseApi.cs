using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BangumiData.Json;

namespace BangumiData
{
    public class BangumiDataBaseApi
    {
        public RootObject? Root { get; private set; }

        public BangumiDataBaseApi(RootObject? root = null)
        {
            Root = root;
        }

        [MemberNotNull(nameof(Root))]
        protected void Init(RootObject root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>
        /// 根据站点名与id获取番剧条目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public SiteMeta GetSiteMeta(string site)
        {
            return Root?.SiteMeta[site] ?? throw new NullReferenceException(nameof(Root));
        }

        /// <summary>
        /// 根据站点名与id获取番剧条目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public Item? GetItemById(string id, string site = "bangumi")
        {
            return Root?.Items.FirstOrDefault(e => e.Sites.Any(s => s.Site == site && s.Id == id));
        }
    }
}
