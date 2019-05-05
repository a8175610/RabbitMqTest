using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.RabbitMQ
{
    public class StoreHelper
    {
        public static ConcurrentDictionary<string, string> StoreList = new ConcurrentDictionary<string, string>();

        public static bool AddOrUpdate(string key, string value)
        {
            if (!string.IsNullOrEmpty(Get(key)))
                Remove(key);
            return StoreList.TryAdd(key, value);
        }

        public static string Get(string key)
        {
            if (StoreList.TryGetValue(key, out string value))
                return value;
            return string.Empty;
        }

        public static string Remove(string key)
        {
            if (StoreList.TryRemove(key, out string value))
                return value;
            return string.Empty;
        }

        public static void Clear()
        {
            StoreList.Clear();
        }
    }
}