using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using BangumiData;
using BangumiData.JsonConverters;
using BangumiData.Models;
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
            Debug.WriteLine(GC.GetTotalMemory(true));
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new CustomDateTimeOffsetConverter());
            options.Converters.Add(new BroadcastConverter());
            _rootObject = JsonSerializer.Deserialize<RootObject>(File.ReadAllBytes(@".\TestData\data.json"), options);
            Debug.WriteLine(GC.GetTotalMemory(true));
        }

        [TestMethod]
        public void TestBroadcast()
        {
            Broadcast val;

            Assert.IsFalse(Broadcast.TryParse("", out val));
            Assert.AreEqual(Broadcast.Empty, val);
            Assert.IsFalse(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P1W", out val));
            Assert.AreEqual(Broadcast.Empty, val);

            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P7D", out val));
            Assert.AreNotEqual(Broadcast.Empty, val);
            Assert.AreEqual(DateTimeOffset.Parse("2020-01-01T13:00:00Z"), val.Next(0));
            Assert.AreEqual(DateTimeOffset.Parse("2020-01-01T13:00:00Z").AddDays(7), val.Next(1));

            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P14D", out val));
            Assert.AreNotEqual(Broadcast.Empty, val);
            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P0D", out val));
            Assert.AreNotEqual(Broadcast.Empty, val);
            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P1M", out val));
            Assert.AreNotEqual(Broadcast.Empty, val);
        }

        [TestMethod]
        public void TestBangumiDataBaseApi()
        {
            ReadData();
            Debug.WriteLine(GC.GetTotalMemory(true));
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
            Debug.WriteLine(GC.GetTotalMemory(true));
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
