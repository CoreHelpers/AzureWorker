using System;
namespace CoreHelpers.Azure.Worker.Clients
{
    public interface IAzureQueueClientPriorityConfiguration : IAzureQueueClientConfiguration
    {
        string PriorityDisplayName { get; }

        int Priority { get; }
    }
}
