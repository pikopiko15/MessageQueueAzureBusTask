using Azure.Identity;
using Azure.Messaging.ServiceBus;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ServiceBusClient client;

        ServiceBusProcessor processor;

        var clientOptions = new ServiceBusClientOptions()
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };
        client = new ServiceBusClient("messagequeuetaskservicebus.servicebus.windows.net",
            new DefaultAzureCredential(), clientOptions);

        processor = client.CreateProcessor("imagesqueue", new ServiceBusProcessorOptions());

        try
        {
            processor.ProcessMessageAsync += MessageHandler;

            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();

            Console.WriteLine("Wait for a minute and then press any key to end the processing");
            Console.ReadKey();

            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");
        }
        finally
        {
            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await processor.DisposeAsync();
            await client.DisposeAsync();
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            

            byte[] chunkData = args.Message.Body.ToArray();

            // Append the chunk to the output file
            using (var fileStream = new FileStream(@"C:\Users\Kemal_Maralov\Desktop\images\receivedimage.jpg", FileMode.Append))
            {
                await fileStream.WriteAsync(chunkData, 0, chunkData.Length);
            }

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}