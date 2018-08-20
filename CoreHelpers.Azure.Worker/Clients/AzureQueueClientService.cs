using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace CoreHelpers.Azure.Worker.Clients
{
	public class AzureQueueClientService
	{
		private IAzureQueueClientConfiguration _configuration { get; set; }
		
		public AzureQueueClientService(IAzureQueueClientConfiguration configuration) 
		{
			_configuration = configuration;
		}
		
        public Task PostMessage<T>(T message, AzureQueueClientRequestOptions requestOptions = null) where T: class 
		{
			// configure
            var account = new CloudStorageAccount(new StorageCredentials(_configuration.StorageAccountName, _configuration.StorageAccountSecret), _configuration.StorageAccountEndpointSuffix, true);
			var client = account.CreateCloudQueueClient();
			
			// initialize a queue reference 
			var taskQueue = client.GetQueueReference(_configuration.StorageQueueName);

            // define the default values
            if (requestOptions == null)
                requestOptions = new AzureQueueClientRequestOptions();
            
            // enque message
            return taskQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)), requestOptions.MessageTimeToLive, requestOptions.VisibilityTimeout, null, null);    
		}
	}
}
