using Dapr.Client;
using Google.Type;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShoppingCartService.Model;
using System.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ShoppingCartService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        string ABSconnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ABS"].ConnectionString;
        string QueueConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Queue"].ConnectionString;

        // GET: api/<CartController>
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            //var daprClient = new DaprClientBuilder().Build();

            //await daprClient.PublishEventAsync<string>("pubsub", "newOrder", "Testing");
            ServiceBusClient client;
            ServiceBusSender sender;

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            client = new ServiceBusClient(ABSconnectionString, clientOptions);
            sender = client.CreateSender(QueueConnectionString);

            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= 5; i++)
            {
                // try adding a message to the batch
                Order newOrder = new Order() { Id = i,  Amount = 1, Name = "Testing" };
                string messageBody = JsonConvert.SerializeObject(newOrder);
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(messageBody)))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of 5 messages has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new string[] { "value1", "value2" };
        }

        // GET api/<CartController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CartController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
     
        }

        // PUT api/<CartController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CartController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
