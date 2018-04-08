using System;
namespace CoreHelpers.Azure.Worker.Clients
{
    public class AzureQueueClientPriorityConfiguration : AzureQueueClientConfiguration, IAzureQueueClientPriorityConfiguration
    {
        public AzureQueueClientPriorityConfiguration(string queueName, int queuePriority, string accountName, string accountSecret)
            : base(queueName, accountName, accountSecret)
        {
            Priority = queuePriority;
            PriorityDisplayName = $"Priority {Priority}";
        }
        
        public AzureQueueClientPriorityConfiguration(string queueName, int queuePriority, string connectionString) 
            :base(queueName, connectionString)
        {
            Priority = queuePriority;
            PriorityDisplayName = $"Priority {Priority}";
        }

        public AzureQueueClientPriorityConfiguration(IAzureQueueClientConfiguration src)
            : base(src)
        {
            Priority = 1;
            PriorityDisplayName = $"Priority {Priority}";
        }

        public AzureQueueClientPriorityConfiguration(IAzureQueueClientPriorityConfiguration src) 
            :base(src)
        {
            Priority = src.Priority;
            PriorityDisplayName = src.PriorityDisplayName;
        }

        public string PriorityDisplayName { get; set; }

        public int Priority { get; set; }
    }
}
