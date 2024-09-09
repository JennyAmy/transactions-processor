using Newtonsoft.Json;
using StackExchange.Redis;

namespace TransactionsProcessor.Services
{
    public class RedisService
    {
        private readonly ILogger<RedisService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _database;
        private readonly TimeSpan _defaultRedisCacheDuration;
        private readonly string RedisPrefix;

        public RedisService(ILogger<RedisService> logger,
            ConnectionMultiplexer connectionMultiplexer, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _database = connectionMultiplexer.GetDatabase();
            _defaultRedisCacheDuration = TimeSpan.FromHours(12.00);
            RedisPrefix = _configuration["Redis:Prefix"]!;
        }

        public T Get<T>(string key)
        {
            try
            {
                var value = _database.StringGet($"{RedisPrefix}-{key}");
                if (!string.IsNullOrEmpty(value))
                    //var jsonString = value.ToString();
                    return JsonConvert.DeserializeObject<T>(value!)!;

                return default(T)!;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while getting value for key {redisKey}", $"RedisPrefix-{key}");
                throw;
            }
        }

        public bool Set<T>(string key, T value, TimeSpan? cacheDuration = null)
        {
            try
            {
                if (cacheDuration == null)
                {
                    cacheDuration = _defaultRedisCacheDuration;
                }
                var serializedValue = JsonConvert.SerializeObject(value);
                return _database.StringSet($"{RedisPrefix}-{key}", serializedValue, cacheDuration.Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while setting value for key {redisKey}", $"RedisPrefix-{key}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                return await _database.KeyDeleteAsync($"{RedisPrefix}-{key}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while getting value for key {redisKey}", $"RedisPrefix-{key}");
                throw;
            }
        }
    }
}
