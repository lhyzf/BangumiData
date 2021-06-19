using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BangumiData.JsonConverters;
using System.Text.Json;

namespace BangumiData
{
    public class BangumiDataApi : BangumiDataBaseApi
    {
        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        private readonly string _directory;
        public string Data_json => Path.Combine(_directory, "data.json");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory">Require read/write folder path to save bangumi-data cache file.</param>
        public BangumiDataApi(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _directory = directory;

            if (File.Exists(Data_json))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.Converters.Add(new CustomDateTimeOffsetConverter());
                var root = JsonSerializer.Deserialize<RootObject>(File.ReadAllBytes(Data_json), options);
                if (root != null)
                {
                    Init(root);
                }
                else
                {

                }
            }
        }
    }
}
