using Confluent.Kafka;
using Ecommerce.model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace Ecommerce.OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(OrderDbContext dbcontext,IKafkaProducer producer) : ControllerBase
    {
        [HttpGet]
        public async Task<List<OrderModel>> GetOrders()
        {
            return await dbcontext.Orders.ToListAsync();
        }

        [HttpPost]
        public async Task<OrderModel> CreateOrder(OrderModel order)
        {
            order.OrderDate = DateTime.UtcNow;
            dbcontext.Orders.Add(order);
            await dbcontext.SaveChangesAsync();

            //produce a message to kafka topic

            await producer.ProduceAsync("order-topic", new Message<string, string>
            {
                Key = order.Id.ToString(),
                Value = JsonConvert.SerializeObject(order)
            });

            return order;
        }
    }
}
