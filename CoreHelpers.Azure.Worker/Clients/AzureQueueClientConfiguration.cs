using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace CoreHelpers.Azure.Worker.Clients
{
	public class AzureQueueClientConfiguration : IAzureQueueClientConfiguration
	{
		public AzureQueueClientConfiguration(string queueName, string accountName, string accountSecret)
		{
			StorageQueueName = queueName;
			StorageAccountName = accountName;
			StorageAccountSecret = accountSecret;
		}
		
		public AzureQueueClientConfiguration(string queueName, string connectionString) 		
		{
			StorageQueueName = queueName;
			
			// parse the connection string
			var storageAccountModel = CloudStorageAccount.Parse(connectionString);
			StorageAccountName = storageAccountModel.Credentials.AccountName;
			StorageAccountSecret = storageAccountModel.Credentials.ExportBase64EncodedKey();
		}				

        public AzureQueueClientConfiguration(IAzureQueueClientConfiguration src) 
            : this(src.StorageQueueName, src.StorageAccountName, src.StorageAccountSecret)
        {}

		public string StorageQueueName { get; set; }

		public string StorageAccountName { get; set; }

		public string StorageAccountSecret { get; set; }

        public string StorageAccountEndpointSuffix { get; set; }
    }
}
