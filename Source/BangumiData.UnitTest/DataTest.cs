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
        private const string OriginalDirectory = @".\TestData";
        private const string TestTempDirectory = @".\TestDataTemp";


        public RootObject ReadData()
        {
            return JsonSerializer.Deserialize<RootObject>(File.ReadAllBytes(Path.Combine(OriginalDirectory, "data.json")), RootObject.SerializerOptions);
        }

        [TestCleanup]
        public void CleanData()
        {
            if (Directory.Exists(TestTempDirectory))
            {
                Directory.Delete(TestTempDirectory, true);
            }
        }

        public void CopyOriginalData(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            foreach (var file in Directory.GetFiles(OriginalDirectory))
            {
                File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), true);
            }
        }

        [TestMethod]
        public void TestBroadcast()
        {
            Broadcast val;

            Assert.IsFalse(Broadcast.TryParse("", out val));
            Assert.IsNull(val);
            Assert.IsFalse(Broadcast.TryParse("R /2020-01-01T13:00:00Z/P1W", out val));
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
            var rootObject = ReadData();
            var baseApi = new BangumiDataBaseApi(rootObject);

            #region Check null values
            foreach (var item in rootObject.SiteMeta)
            {
                Assert.IsNotNull(item.Value.Title);
                Assert.IsNotNull(item.Value.UrlTemplate);
                Assert.IsNotNull(item.Value.Type);
                //Assert.IsNotNull(item.Value.Regions);
            }
            foreach (var item in rootObject.Items)
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
            Assert.IsTrue(rootObject.Items.Any(it => it.End != null));
            Assert.IsTrue(rootObject.Items.Any(it => it.Broadcast != null));
            Assert.IsTrue(rootObject.Items.Any(it => it.Comment != null));
            Assert.IsTrue(rootObject.Items.Any(it => it.Sites.Any(it => it.Id != null)));
            Assert.IsTrue(rootObject.Items.Any(it => it.Sites.Any(it => it.Begin != null)));
            Assert.IsTrue(rootObject.Items.Any(it => it.Sites.Any(it => it.Broadcast != null)));
            Assert.IsTrue(rootObject.Items.Any(it => it.Sites.Any(it => it.Url != null)));
            Assert.IsTrue(rootObject.Items.Any(it => it.Sites.Any(it => it.Comment != null)));
            #endregion

            foreach (var item in rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    baseApi.GetItemById(x.Id);
                }
            }
            Debug.WriteLine(GC.GetTotalMemory(true));
        }

        [TestMethod]
        public async Task TestBangumiDataApi()
        {
            var rootObject = ReadData();
            CopyOriginalData(TestTempDirectory);
            var api = new BangumiDataApi(new FilePersistence(TestTempDirectory));
            //ReadData();
            api.GetEnabledSites();
            api.GetDisabledSites();
            foreach (var item in rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    await foreach (var site in api.GetAirSitesByBangumiIdAsync(x.Id)) { }
                }
            }
            api.UseBiliApp = true;
            foreach (var item in rootObject.Items)
            {
                var x = item.Sites.FirstOrDefault(it => it.Site == "bangumi");
                if (x != null)
                {
                    await foreach (var site in api.GetAirSitesByBangumiIdAsync(x.Id)) { }
                }
            }
        }

        [TestMethod]
        public async Task TestBiliSeasonIdMapper()
        {
            var map = new BiliSeasonIdMapper(null);
            var id = await map.GetSeasonIdAsync("28233896");
            Assert.AreEqual("38214", id);
        }

        [TestMethod]
        public async Task TestBangumiDataApiUpdate()
        {
            CopyOriginalData(TestTempDirectory);
            var api = new BangumiDataApi(new FilePersistence(TestTempDirectory));
            var ret = await api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            Assert.IsFalse(string.IsNullOrEmpty(ret.Version));
            if (ret.Version != api.Version)
            {
                Assert.AreEqual(true, await api.DownloadLatestVersion(ret.Version));
            }
        }

        [TestMethod]
        public async Task TestBangumiDataApiAutoCheck()
        {
            CopyOriginalData(TestTempDirectory);
            var api = new BangumiDataApi(new FilePersistence(TestTempDirectory));
            var ret = await api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            api.AutoCheck = true;
            api.CheckInterval = 0;
            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);
            api = new BangumiDataApi(new FilePersistence(TestTempDirectory),
                new EventHandler((s, e) =>
                {
                    Assert.AreNotEqual(ret.Version, api.Version);
                    semaphoreSlim.Release();
                }));
            Assert.IsTrue(await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(10)));
        }

        [TestMethod]
        public async Task TestBangumiDataApiAutoUpdate()
        {
            CopyOriginalData(TestTempDirectory);
            var api = new BangumiDataApi(new FilePersistence(TestTempDirectory));
            var ret = await api.TryGetLatestVersion();
            Assert.IsTrue(ret.IsSuccess);
            api.AutoCheck = true;
            api.AutoUpdate = true;
            api.CheckInterval = 0;
            bool flag = false;
            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);
            api = new BangumiDataApi(new FilePersistence(TestTempDirectory),
                new EventHandler((s, e) =>
                {
                    Assert.AreNotEqual(ret.Version, api.Version);
                }),
                new EventHandler((s, e) =>
                {
                    flag = true;
                }),
                new EventHandler((s, e) =>
                {
                    Assert.IsTrue(flag);
                    semaphoreSlim.Release();
                })
            );
            Assert.IsTrue(await semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(20)));
        }

        [TestMethod]
        public async Task TestBangumiDataApiFirstUsage()
        {
            var api = new BangumiDataApi(new FilePersistence(TestTempDirectory));
            Assert.AreEqual(true, await api.DownloadLatestVersion());
            api.UseBiliApp = true;
            var sites = new List<SiteInfo>();
            await foreach (var site in api.GetAirSitesByBangumiIdAsync("297954"))
            {
                sites.Add(site);
            }
            var biliSite = sites.FirstOrDefault(it => it.Site.StartsWith("哔哩"));
            Assert.IsNotNull(biliSite);
            Assert.IsTrue(biliSite.Url == "bilibili://bangumi/season/38214");
        }

        [TestMethod]
        public async Task TestBangumiDataApiWithoutIPersistence()
        {
            var api = new BangumiDataApi(null);
            Assert.AreEqual(true, await api.DownloadLatestVersion());
            api.UseBiliApp = true;
            var sites = new List<SiteInfo>();
            await foreach (var site in api.GetAirSitesByBangumiIdAsync("297954"))
            {
                sites.Add(site);
            }
            var biliSite = sites.FirstOrDefault(it => it.Site.StartsWith("哔哩"));
            Assert.IsNotNull(biliSite);
            Assert.IsTrue(biliSite.Url == "bilibili://bangumi/season/38214");
        }
    }
}
