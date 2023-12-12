using Azure.Identity;
using Azure.Messaging.ServiceBus;

internal class Program
{
    private static async Task Main(string[] args)
    {
        ServiceBusClient client;

        ServiceBusSender sender;

        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };

        client = new ServiceBusClient(
            "messagequeuetaskservicebus.servicebus.windows.net",
            new DefaultAzureCredential(),
            clientOptions);
        sender = client.CreateSender("imagesqueue");

        await SendChunksToServiceBus(@"C:\Users\Kemal_Maralov\Desktop\images\annie-spratt-wW-1-BRNbHM-unsplash.jpg", client, sender);

        Console.WriteLine("Press any key to end the application");
        Console.ReadKey();
    }

    private static async Task SendChunksToServiceBus(string filePath, ServiceBusClient serviceBusClient, ServiceBusSender sender)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[128 * 1024];
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] chunkData = new byte[bytesRead];
                    Array.Copy(buffer, chunkData, bytesRead);

                    var message = new ServiceBusMessage(chunkData);
                    await sender.SendMessageAsync(message);
                }
            }

            Console.WriteLine("File chunks sent to Service Bus successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}