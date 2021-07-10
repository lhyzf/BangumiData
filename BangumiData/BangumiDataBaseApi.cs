using System.Linq;
using BangumiData.Models;

namespace BangumiData
{
    public class BangumiDataBaseApi
    {
        public RootObject Root { get; private set; }

        protected BangumiDataBaseApi() { }

        protected void Init(RootObject root)
        {
            Root = root;
        }

        public BangumiDataBaseApi(RootObject root)
        {
            Root = root;
        }

        /// <summary>
        /// 根据站点名与id获取番剧条目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public Item? GetItemById(string id, string siteName = "bangumi")
        {
            return Root.Items.FirstOrDefault(e => e.Sites.Any(s => s.SiteName == siteName && s.Id == id));
        }
    }
}
