//using Microsoft.EntityFrameworkCore.Storage;
//using Newtonsoft.Json;
//using StackExchange.Redis;

//namespace AddressBook.Cache
//{
//    public class CacheService : ICacheService
//    {
//        private readonly StackExchange.Redis.IDatabase _database;

//        //public CacheService(IConnectionMultiplexer connectionMultiplexer)
//        //{
//        //    _database = connectionMultiplexer.GetDatabase();
//        //}
//        public CacheService(IConnectionMultiplexer redis)
//        {
//            _database = redis.GetDatabase();
//        }

//        public bool CheckConnection()
//        {
//            try
//            {
//                return _database.Multiplexer.IsConnected;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($" Redis CheckConnection Error: {ex.Message}");
//                return false;
//            }
//        }


//        public T GetData<T>(string key)
//        {
//            var value = _database.StringGet(key);
//            if (!value.IsNullOrEmpty)
//            {
//                return JsonConvert.DeserializeObject<T>(value);
//            }
//            return default;
//        }

//        public bool RemoveData(string key)
//        {
//            return _database.KeyExists(key) && _database.KeyDelete(key);
//        }

//        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
//        {
//            TimeSpan expiryTime = expirationTime - DateTimeOffset.Now;
//            return _database.StringSet(key, JsonConvert.SerializeObject(value), expiryTime);
//        }
//    }
//}


using Newtonsoft.Json;
using StackExchange.Redis;

namespace AddressBook.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _database;

        public CacheService(IConnectionMultiplexer redis)
        {
            if (redis == null) throw new ArgumentNullException(nameof(redis));
            _database = redis.GetDatabase();
        }

        public bool CheckConnection()
        {
            return _database.Multiplexer.IsConnected;
        }

        public T GetData<T>(string key)
        {
            var value = _database.StringGet(key);
            if (!value.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            return default;
        }

        public bool RemoveData(string key)
        {
            return _database.KeyDelete(key);
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            TimeSpan expiryTime = expirationTime - DateTimeOffset.Now;
            return _database.StringSet(key, JsonConvert.SerializeObject(value), expiryTime);
        }
    }
}

