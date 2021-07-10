using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BangumiData
{
    public static class FileHelper
    {
        /// <summary>
        /// 异步读文件，文件不存在将返回空字符串
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        public static async Task<string> ReadTextAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var reader = File.OpenText(filePath))
                {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 异步读文件，文件不存在将返回 byte[0]
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        public static async Task<byte[]> ReadBytesAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var reader = File.OpenRead(filePath))
                {
                    var result = new byte[reader.Length];
                    await reader.ReadAsync(result, 0, (int)reader.Length).ConfigureAwait(false);
                    return result;
                }
            }
            return new byte[0];
        }

        /// <summary>
        /// 异步读文件，文件不存在将返回 null
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <returns></returns>
        public static async Task<T?> ReadAsync<T>(string filePath, JsonSerializerOptions? options = null)
        {
            var obj = default(T);
            try
            {
                var bytes = await ReadBytesAsync(filePath).ConfigureAwait(false);
                if (bytes.Length > 0)
                {
                    obj = JsonSerializer.Deserialize<T>(bytes, options);
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                DeleteFile(filePath);
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

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <param name="data">待写入文本</param>
        /// <returns></returns>
        public static async Task WriteTextAsync(string filePath, string data)
        {
            if (!File.Exists(filePath))
            {
                using var f = File.Create(filePath);
            }
            var tempFile = filePath + ".temp";
            using (var writer = File.CreateText(tempFile))
            {
                await writer.WriteAsync(data).ConfigureAwait(false);
            }
            File.Replace(tempFile, filePath, null, true);
        }

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <param name="data">待写入 byte[]</param>
        /// <returns></returns>
        public static async Task WriteBytesAsync(string filePath, byte[] data)
        {
            if (!File.Exists(filePath))
            {
                using var f = File.Create(filePath);
            }
            var tempFile = filePath + ".temp";
            using (var writer = File.Create(tempFile))
            {
                await writer.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            }
            File.Replace(tempFile, filePath, null, true);
        }

        /// <summary>
        /// 异步写文件
        /// </summary>
        /// <param name="filePath">文件路径全名</param>
        /// <param name="data">待写入数据</param>
        /// <returns></returns>
        public static async Task WriteAsync<T>(string filePath, T data, JsonSerializerOptions? options = null)
        {
            try
            {
                await WriteBytesAsync(filePath, JsonSerializer.SerializeToUtf8Bytes(data, options)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
        }

        /// <summary>
        /// 删除存在的文件
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
