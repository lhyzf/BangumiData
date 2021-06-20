using System;
using System.Diagnostics;
using System.Linq;
using BangumiData.Models;

namespace BangumiData
{
    public class BangumiDataBaseApi
    {
        private RootObject _rootObject;

        protected BangumiDataBaseApi() { }

        protected void Init(RootObject rootObject)
        {
            _rootObject = rootObject;
        }

        public BangumiDataBaseApi(RootObject rootObject)
        {
            _rootObject = rootObject;
        }

        public Item? GetItemById(string id, string siteName = "bangumi")
        {
            return _rootObject.Items.FirstOrDefault(e => e.Sites.Any(s => s.SiteName == siteName && s.Id == id));
        }

    }
}
