using Ecommerce.model;
using Ecommerce.ProductService.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(ProductDbContext dbcontext) : ControllerBase
    {
        [HttpGet]
        public async Task<List<ProductModel>> GetProducts()
        {
            return await dbcontext.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductModel>> GetProduct(int id)
        {
            var product = await dbcontext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }
    }
}
