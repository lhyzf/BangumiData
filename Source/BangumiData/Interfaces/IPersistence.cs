using System;

namespace BangumiData.Interfaces
{
    public interface IPersistence : IDisposable
    {
        void Save<T>(string key, T data);
        T? Read<T>(string key);
    }
}
