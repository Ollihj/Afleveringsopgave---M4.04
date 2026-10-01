using Microsoft.Azure.Cosmos;
using SupportWebApp.Models;

namespace SupportWebApp.Services;

// Håndterer forbindelsen til CosmosDB
public class CosmosService
{
    private readonly Container _container;

    public CosmosService(IConfiguration config)
    {
        string? connectionString = config["CosmosDb:ConnectionString"];
        string? databaseName = config["CosmosDb:DatabaseName"];
        string? containerName = config["CosmosDb:ContainerName"];

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("CosmosDb:ConnectionString mangler - se Readme.md");
        }

        CosmosClient client = new CosmosClient(connectionString);
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task AddSupportMessageAsync(SupportMessage message)
    {
        await _container.CreateItemAsync(message, new PartitionKey(message.Category));
    }

    public async Task<List<SupportMessage>> GetAllSupportMessagesAsync()
    {
        List<SupportMessage> result = new List<SupportMessage>();
        FeedIterator<SupportMessage> iterator = _container.GetItemQueryIterator<SupportMessage>("SELECT * FROM c");

        while (iterator.HasMoreResults)
        {
            FeedResponse<SupportMessage> response = await iterator.ReadNextAsync();
            result.AddRange(response);
        }

        // Nyeste henvendelser først
        return result.OrderByDescending(m => m.CreatedAt).ToList();
    }
}
