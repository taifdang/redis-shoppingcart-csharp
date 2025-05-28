using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Server.API.Data;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Sevices;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Server.API.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class productController : ControllerBase
    {
        private readonly IProductRepository _repository;
        public productController(IProductRepository repository)
        {
            _repository = repository;      
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _repository.getAll();           
            return Ok(data);
          
        }
        [HttpGet("reset")]
        public async Task<IActionResult> Reset()
        {
            var data =  await _repository.reset();
            return Ok(data);
        }
    }
}
