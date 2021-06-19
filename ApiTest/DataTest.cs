using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using BangumiData;
using BangumiData.JsonConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiTest
{
    [TestClass]
    public class DataTest
    {
        private RootObject _rootObject;
        private BangumiDataBaseApi _baseApi;
        private BangumiDataApi _api;

        public void ReadData()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new CustomDateTimeOffsetConverter());
            _rootObject = JsonSerializer.Deserialize<RootObject>(File.ReadAllBytes(@".\TestData\data.json"), options);
        }

        [TestMethod]
        public void TestBangumiDataBaseApi()
        {
            ReadData();
            _baseApi = new BangumiDataBaseApi(_rootObject);

            #region Check null values
            foreach (var item in _rootObject.SiteMeta)
            {
                Assert.IsNotNull(item.Value.Title);
                Assert.IsNotNull(item.Value.UrlTemplate);
                Assert.IsNotNull(item.Value.Type);
                //Assert.IsNotNull(item.Value.Regions);
            }
            foreach (var item in _rootObject.Items)
            {
                Assert.IsNotNull(item.Title);
                Assert.IsNotNull(item.TitleTranslate);
                Assert.IsNotNull(item.Type);
                Assert.IsNotNull(item.Lang);
                Assert.IsNotNull(item.OfficialSite);
                Assert.IsNotNull(item.Begin);
                //Assert.IsNotNull(item.End);
                Assert.IsNotNull(item.Sites);
                //Assert.IsNotNull(item.Broadcast);
                //Assert.IsNotNull(item.Comment);
                foreach (var site in item.Sites)
                {
                    Assert.IsNotNull(site.SiteName);
                    //Assert.IsNotNull(site.Id);
                    //Assert.IsNotNull(site.Begin);
                    //Assert.IsNotNull(site.Broadcast);
                    //Assert.IsNotNull(site.Url);
                    //Assert.IsNotNull(site.Comment);
                }
            }
            #endregion


            foreach (var item in _rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.SiteName == "bangumi");
                if (x != null)
                {
                    _baseApi.GetItemById(x.Id);
                }
            }
        }

        [TestMethod]
        public void TestBangumiDataApi()
        {
            _api = new BangumiDataApi(@".\TestData");
            ReadData();
            foreach (var item in _rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.SiteName == "bangumi");
                if (x != null)
                {
                    _api.GetItemById(x.Id);
                }
            }
        }
    }
}
