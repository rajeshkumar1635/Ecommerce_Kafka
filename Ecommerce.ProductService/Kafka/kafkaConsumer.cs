using Confluent.Kafka;
using Ecommerce.model;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;

namespace Ecommerce.ProductService.Kafka
{
    public class kafkaConsumer(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                _ = ConsumeAsync("order-topic", stoppingToken);
            });
        }

        public async Task ConsumeAsync(string topic, CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "product-service-group",
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            };
            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);
                // process the message
                var order = JsonConvert.DeserializeObject<OrderModel>(consumeResult.Message.Value);

                using var scope = scopeFactory.CreateScope();

                var dbcontext = scope.ServiceProvider.GetRequiredService<Data.ProductDbContext>();
                var product = await dbcontext.Products.FindAsync(order.ProductId);  

                if (product != null)
                {
                    product.Quantity -= order.Quantity;
                    await dbcontext.SaveChangesAsync();
                }
               
            }
            consumer.Close();
        }
    }
}
