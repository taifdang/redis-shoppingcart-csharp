
using Server.API.Common;
using Server.API.Data;
using Server.API.Sevices;
using StackExchange.Redis;

namespace Server.API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly GetUserContext _userContext;
        private readonly IConnectionMultiplexer _redis;
        private readonly DatabaseContext _db;
        private readonly IConfiguration _config;
        public CartRepository(GetUserContext userContext, IConnectionMultiplexer redis, DatabaseContext db, IConfiguration config)
        {
            _userContext = userContext;
            _redis = redis;
            _db = db;
            _config = config;
        }

        public async Task<StateResponseModel<string>> deleteCart(Guid producId)
        {
            try
            {
                var cartKey = _userContext.getCartKey();    
                var product = _db.products.Find(producId);
                if (product is null) return StateResponseModel<string>.Error("Not found product");

                var db = _redis.GetDatabase();
                var quantityInCart = db.HashGet(cartKey, producId.ToString());

                db.HashDelete(cartKey, producId.ToString());
                //update stock
                product.stock += (int)quantityInCart;
                _db.SaveChanges();
                //remove cache*
                await db.KeyDeleteAsync(_config["RedisCache:productKey"]);
                return StateResponseModel<string>.Success();
            }
            catch
            {
                return StateResponseModel<string>.Error();
            }
        }

        public async Task<StateResponseModel<string>> emptyCart()
        {
            try
            {
                var cartKey = _userContext.getCartKey();

                var db = _redis.GetDatabase();
                var cartList = db.HashGetAll(cartKey);

                foreach (var cartItem in cartList)
                {
                    var hashToGuid = Guid.Parse(cartItem.Name.ToString());
                    var item = _db.products.Find(hashToGuid);
                    if (item is null) return StateResponseModel<string>.Error("Not found product");

                    var productInCart = (int)cartItem.Value;
                    db.HashDelete(cartKey, cartItem.Name);
                    //update stock
                    item!.stock += productInCart;
                }
                //atomic database
                _db.SaveChanges();
                //remove cache*
                //await db.KeyDeleteAsync(_config["RedisCache:productKey"]);
                return StateResponseModel<string>.Success();
            }
            catch
            {
                return StateResponseModel<string>.Error();
            }
        }       
        public async Task<StateResponseModel<List<CartItemDTO>>> getCart()
        {
            try
            {
                var cartkey = _userContext.getCartKey();
                var db = _redis.GetDatabase();

                var data = await db.HashGetAllAsync(cartkey);
                //var dataJson = JsonConvert.SerializeObject(data);
                var cartItemList = data.Select(x => new CartItemDTO
                {
                    productId = Guid.Parse(x.Name.ToString()),
                    quantity = (int)x.Value
                }).ToList();

                return StateResponseModel<List<CartItemDTO>>.Success(cartItemList);
            }
            catch
            {
                return StateResponseModel<List<CartItemDTO>>.Error();
            }
        }

        public async Task<StateResponseModel<string>> updateCart(Guid productId, int? quantity, int? incrementBy)
        {
            try
            {
                var cartkey = _userContext.getCartKey();
                var db = _redis.GetDatabase();
                var productInStock = await _db.products.FindAsync(productId);
                if (productInStock is null) return StateResponseModel<string>.Error("Not find product");
                var stock = productInStock.stock;
                //non-exist => convert to (int)quantityInCart = 0
                var quantityInCart = db.HashGet(cartkey, productId.ToString());
                //non-exist => create new [quantity]
                if (quantity.HasValue)
                {
                    if (quantity <= 0) return StateResponseModel<string>.Error("Quantity isn't valid");
                    var newStock = stock - quantity;
                    if (newStock < 0) return StateResponseModel<string>.Error("Quantity in stock not enough");

                    db.HashSet(cartkey, productId.ToString(), quantity + (int)quantityInCart);
                    //update stock
                    productInStock.stock = (int)newStock;
                    _db.SaveChanges();
                }
                //exist => update (+1,-1) [incrementBy]
                if (incrementBy.HasValue)
                {
                    if (incrementBy != -1 && incrementBy != 1) return StateResponseModel<string>.Error("IncrementBy must be 1 or -1");
                    var quantityAfterIncrement = (int)quantityInCart + incrementBy;
                    if (quantityAfterIncrement <= 0 || stock - incrementBy < 0) return StateResponseModel<string>.Error("Can't decrement stock to 0");                  
                    db.HashIncrement(cartkey, productId.ToString(), (int)incrementBy);
                    //update stock
                    productInStock.stock -= (int)incrementBy;
                    _db.SaveChanges();
                }
                //remove cache*
                await db.KeyDeleteAsync(_config["RedisCache:productKey"]);
                return StateResponseModel<string>.Success();
            }
            catch
            {
                return StateResponseModel<string>.Error("Isn't Valid");
            }
        }
    }
}
