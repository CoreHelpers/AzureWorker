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
		
        public async Task PostMessage<T>(T message, AzureQueueClientRequestOptions requestOptions = null) where T: class 
		{			
            await this.PostMessage(JsonConvert.SerializeObject(message), requestOptions);
		}

        public async Task PostMessage(string messageText, AzureQueueClientRequestOptions requestOptions = null)
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
            try
            {
                await taskQueue.AddMessageAsync(new CloudQueueMessage(messageText), requestOptions.MessageTimeToLive, requestOptions.VisibilityTimeout, null, null);
            } catch(StorageException e)
            {
                // check if we have something else as not exists
                if (!e.Message.Contains("The specified queue does not exist"))
                    throw e;

                // create the queue
                await taskQueue.CreateAsync();

                // try it again
                await PostMessage(messageText, requestOptions);
            }
        }

        public async Task<T> PopMessage<T>() where T : class
        {
            try
            {
                // configure
                var account = new CloudStorageAccount(new StorageCredentials(_configuration.StorageAccountName, _configuration.StorageAccountSecret), _configuration.StorageAccountEndpointSuffix, true);
                var client = account.CreateCloudQueueClient();

                // initialize a queue reference 
                var taskQueue = client.GetQueueReference(_configuration.StorageQueueName);

                // retrieve the message
                CloudQueueMessage retrievedMessage = await taskQueue.GetMessageAsync();
                if (retrievedMessage == null)
                    return null;

                // delete the received message 
                await taskQueue.DeleteMessageAsync(retrievedMessage);

                // return message
                return JsonConvert.DeserializeObject<T>(retrievedMessage.AsString);
            } catch (StorageException e)
            {
                if (e.Message.Contains("The specified queue does not exist"))
                    return null;

                throw e;
            }                           
        }
    }
}
