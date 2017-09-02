using System;
namespace CoreHelpers.Azure.Worker.Clients
{
	public interface IAzureQueueClientConfiguration
	{
		string StorageQueueName { get; }
		string StorageAccountName { get; }
		string StorageAccountSecret { get; }
	}
}
