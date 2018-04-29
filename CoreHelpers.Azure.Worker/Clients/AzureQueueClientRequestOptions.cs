using System;
namespace CoreHelpers.Azure.Worker.Clients
{
    public class AzureQueueClientRequestOptions
    {
        public TimeSpan? VisibilityTimeout { get; set; } = null;

        public TimeSpan? MessageTimeToLive { get; set; } = null;
    }
}
