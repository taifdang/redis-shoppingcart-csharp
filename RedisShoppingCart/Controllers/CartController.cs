using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.API.Repositories;
using Server.API.Sevices;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Server.API.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {    
        private readonly ICartRepository _repository;
        public CartController(ICartRepository repository)
        {          
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {        
            var data = await _repository.getCart();
            return Ok(data);
        }       
        [HttpPost("update-CartItem")]
        public async Task<IActionResult> UpdateCart(Guid product_id,int? quantity, int? incrementBy)
        {
            if (quantity.HasValue && incrementBy.HasValue) return BadRequest("Only a request");       
            var data = await _repository.updateCart(product_id, quantity, incrementBy);
            return Ok(data);
        }
        [HttpDelete("delete-cartItem")]
        public async Task<IActionResult> DeleteCart(Guid producId)
        {
            var data = await _repository.deleteCart(producId);
            return Ok(data);
        }
        [HttpDelete("empty-cart")]
        public async Task<IActionResult> EmptyCart()
        {
            var data = await _repository.emptyCart();
            return Ok(data);
        }
    }
}
