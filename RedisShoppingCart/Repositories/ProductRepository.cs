
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Server.API.Data;
using StackExchange.Redis;
using Server.API.Helpers;
using Server.API.Models;
using Server.API.Common;
using System.Threading.Tasks;

namespace Server.API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly DatabaseContext _db;
        private readonly IDistributedCache _distributed;
        private readonly IConfiguration _config;
        public ProductRepository(
            IConnectionMultiplexer redis, 
            DatabaseContext db,
            IDistributedCache distributed,
            IConfiguration config
            )
        {
            _redis = redis;
            _db = db;
            _distributed = distributed;
            _config = config;
        }

        public async Task<StateResponseModel<List<Products>>> getAll()
        {
            try
            {
                if (_distributed.TryGetValue(_config["RedisCache:productKey"], out List<Products>? products))
                {
                    Console.WriteLine("Find cache");
                }
                else
                {
                    products = await _db.products.ToListAsync();
                    //set cache
                    var cacheOptions = new DistributedCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(120));
                    await _distributed.SetAsync(_config["RedisCache:productKey"], products, cacheOptions);
                }
                return StateResponseModel<List<Products>>.Success(products);
            }
            catch
            {
                return StateResponseModel<List<Products>>.Error();
            }
        }

        public async Task<StateResponseModel<string>> reset()
        {
            try
            {
                var db = _redis.GetDatabase();
                var cart = db.Multiplexer.GetServer(_redis.GetEndPoints().First()).Keys(pattern: "cart:user*", pageSize: 2);
                foreach (var key in cart)
                {
                    await db.KeyDeleteAsync(key);
                }
                //reset products => stock = 10
                await _db.products.ExecuteUpdateAsync(s => s.SetProperty(e => e.stock, 10));
                //delete productList cache old
                await db.KeyDeleteAsync(_config["RedisCache:productKey"]);
                return StateResponseModel<string>.Success();
            }
            catch
            {
                return StateResponseModel<string>.Error();
            }

        }
    }
}
