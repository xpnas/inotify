using Inotify;
using Inotify.Data;
using Inotify.Data.Models;
using Inotify.Sends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Inotify.Sends
{
    public class SendCacheStore
    {
        private static readonly System.Runtime.Caching.MemoryCache m_cache;

        private static readonly Dictionary<string, string> m_systemInfos;

        static SendCacheStore()
        {
            m_cache = new System.Runtime.Caching.MemoryCache("CacheStore");
            m_systemInfos = DBManager.Instance.DBase.Query<SystemInfo>().ToList().ToDictionary(e => e.key, e => e.Value);
        }

        public static string Get(string key)
        {
            object obj = m_cache.Get(key);
            if (obj != null && obj is string)
                return obj as string;
            return null;

        }

        public static void Set(string key, string value, DateTimeOffset offset)
        {
            m_cache.Set(key, value, offset);
        }

        public static string GetSystemValue(string key)
        {
            if (m_systemInfos.ContainsKey(key))
                return m_systemInfos[key];
            return "";

        }

        public static void SetSystemValue(string key, string value)
        {
            m_systemInfos[key] = value ?? "";
            var systemInfo = new SystemInfo(key, m_systemInfos[key]);
            if (DBManager.Instance.DBase.Query<SystemInfo>().Where(e => e.key == key).Any())
            {
                DBManager.Instance.DBase.Delete(systemInfo);
            }
            DBManager.Instance.DBase.Insert(systemInfo);
        }
    }
}
