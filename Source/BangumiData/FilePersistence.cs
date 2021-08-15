using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using BangumiData.Interfaces;
using BangumiData.Json;

namespace BangumiData
{
    public class FilePersistence : IPersistence
    {
        private readonly string _directory;

        public FilePersistence(string directory)
        {
            _directory = directory;
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
        }

        private string GetFullFilename(string key)
        {
            return Path.Combine(_directory, key) + ".json";
        }

        /// <summary>
        /// 删除存在的文件
        /// </summary>
        /// <param name="filename">文件完整路径</param>
        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        public T? Read<T>(string key)
        {
            var obj = default(T);
            var filename = GetFullFilename(key);
            if (!File.Exists(filename))
            {
                return obj;
            }
            try
            {
                var bytes = File.ReadAllBytes(filename);
                if (bytes.Length > 0)
                {
                    obj = JsonSerializer.Deserialize<T>(bytes, RootObject.SerializerOptions);
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                DeleteFile(filename);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
            return obj;
        }

        public void Save<T>(string key, T data)
        {
            var filename = GetFullFilename(key);
            var tempFile = filename + ".temp";
            File.WriteAllBytes(tempFile, JsonSerializer.SerializeToUtf8Bytes(data, RootObject.SerializerOptions));
            if (!File.Exists(filename))
            {
                File.Move(tempFile, filename);
            }
            else
            {
                File.Replace(tempFile, filename, null, true);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
