using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BangumiData;
using BangumiData.Json;
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
            _rootObject = JsonSerializer.Deserialize<RootObject>(File.ReadAllBytes(@".\TestData\data.json"), RootObject.SerializerOptions);
            Debug.WriteLine(GC.GetTotalMemory(true));
        }

        [TestMethod]
        public void TestBroadcast()
        {
            Broadcast val;

            Assert.IsFalse(Broadcast.TryParse("", out val));
            Assert.IsNull(val);
            Assert.IsFalse(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P1W", out val));
            Assert.IsNull(val);

            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P7D", out val));
            Assert.IsNotNull(val);
            Assert.AreEqual("R/2020-01-01T13:00:00Z/P7D", val.ToString());
            Assert.AreEqual(DateTimeOffset.Parse("2020-01-01T13:00:00Z"), val.Next(0));
            Assert.AreEqual(DateTimeOffset.Parse("2020-01-01T13:00:00Z").AddDays(7), val.Next(1));

            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P14D", out val));
            Assert.IsNotNull(val);
            Assert.AreEqual("R/2020-01-01T13:00:00Z/P14D", val.ToString());
            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P0D", out val));
            Assert.IsNotNull(val);
            Assert.AreEqual("R/2020-01-01T13:00:00Z/P0D", val.ToString());
            Assert.IsTrue(Broadcast.TryParse("R/2020-01-01T13:00:00Z/P1M", out val));
            Assert.IsNotNull(val);
            Assert.AreEqual("R/2020-01-01T13:00:00Z/P1M", val.ToString());
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
                    Assert.IsNotNull(site.Site);
                    //Assert.IsNotNull(site.Id);
                    //Assert.IsNotNull(site.Begin);
                    //Assert.IsNotNull(site.Broadcast);
                    //Assert.IsNotNull(site.Url);
                    //Assert.IsNotNull(site.Comment);
                }
            }
            Assert.IsTrue(_rootObject.Items.Any(it => it.End != null));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Broadcast != null));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Comment != null));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Sites.Any(it => it.Id != null)));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Sites.Any(it => it.Begin != null)));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Sites.Any(it => it.Broadcast != null)));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Sites.Any(it => it.Url != null)));
            Assert.IsTrue(_rootObject.Items.Any(it => it.Sites.Any(it => it.Comment != null)));
            #endregion

            foreach (var item in _rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    _baseApi.GetItemById(x.Id);
                }
            }
            Debug.WriteLine(GC.GetTotalMemory(true));
        }

        [TestMethod]
        public async Task TestBangumiDataApi()
        {
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"));
            ReadData();
            _api.GetEnabledSites();
            _api.GetDisabledSites();
            foreach (var item in _rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    await foreach (var site in _api.GetAirSitesByBangumiIdAsync(x.Id)) { }
                }
            }
            _api.UseBiliApp = true;
            foreach (var item in _rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    await foreach (var site in _api.GetAirSitesByBangumiIdAsync(x.Id)) { }
                }
            }
        }

        [TestMethod]
        public async Task TestBiliSeasonIdMapper()
        {
            var map = new BiliSeasonIdMapper(new FilePersistence(@".\TestData"));
            var id = await map.GetSeasonIdAsync("28233896");
            Assert.AreEqual("38214", id);
        }

        [TestMethod]
        public async Task TestBangumiDataApiUpdate()
        {
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"));
            var ret = await _api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            Assert.IsFalse(string.IsNullOrEmpty(ret.Version));
            if (ret.Version != _api.Version)
            {
                Assert.AreEqual(true, await _api.DownloadLatestVersion(ret.Version));
            }
        }

        [TestMethod]
        public async Task TestBangumiDataApiAutoCheck()
        {
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"));
            var ret = await _api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            _api.AutoCheck = true;
            _api.CheckInterval = 0;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"),
                new EventHandler((s, e) =>
                {
                    Assert.AreNotEqual(ret.Version, _api.Version);
                    autoEvent.Set();
                }));
            Assert.IsTrue(autoEvent.WaitOne(TimeSpan.FromSeconds(10)));
        }

        [TestMethod]
        public async Task TestBangumiDataApiAutoUpdate()
        {
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"));
            var ret = await _api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            _api.AutoCheck = true;
            _api.AutoUpdate = true;
            _api.CheckInterval = 0;
            bool flag = false;
            AutoResetEvent autoEvent = new AutoResetEvent(false);
            _api = new BangumiDataApi(new FilePersistence(@".\TestData"),
                new EventHandler((s, e) =>
                {
                    Assert.AreNotEqual(ret.Version, _api.Version);
                }),
                new EventHandler((s, e) =>
                {
                    flag = true;
                }),
                new EventHandler((s, e) =>
                {
                    Assert.IsTrue(flag);
                    autoEvent.Set();
                })
            );
            Assert.IsTrue(autoEvent.WaitOne(TimeSpan.FromSeconds(20)));
        }

        [TestMethod]
        public async Task TestBangumiDataApiFirstUsage()
        {
            _api = new BangumiDataApi(new FilePersistence(@".\TestData2"));
            Assert.AreEqual(true, await _api.DownloadLatestVersion());
            _api.UseBiliApp = true;
            var sites = new List<SiteInfo>();
            await foreach (var site in _api.GetAirSitesByBangumiIdAsync("297954"))
            {
                sites.Add(site);
            }
            var biliSite = sites.FirstOrDefault(it => it.Site.StartsWith("ﬂŸ¡®"));
            Assert.IsNotNull(biliSite);
            Assert.IsTrue(biliSite.Url == "bilibili://bangumi/season/38214");
            Directory.Delete(@".\TestData2", true);
        }

        [TestMethod]
        public async Task TestBangumiDataApiWithoutIPersistence()
        {
            _api = new BangumiDataApi(null);
            Assert.AreEqual(true, await _api.DownloadLatestVersion());
            _api.UseBiliApp = true;
            var sites = new List<SiteInfo>();
            await foreach (var site in _api.GetAirSitesByBangumiIdAsync("297954"))
            {
                sites.Add(site);
            }
            var biliSite = sites.FirstOrDefault(it => it.Site.StartsWith("ﬂŸ¡®"));
            Assert.IsNotNull(biliSite);
            Assert.IsTrue(biliSite.Url == "bilibili://bangumi/season/38214");
        }
    }
}
