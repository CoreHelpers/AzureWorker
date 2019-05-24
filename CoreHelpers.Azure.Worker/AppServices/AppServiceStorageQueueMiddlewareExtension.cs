using System;
using System.Threading.Tasks;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using CoreHelpers.Azure.Worker.Clients;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public static class AppServiceStorageQueueMiddlewareExtension	
	{
		public static IWorkerApplicationBuilder UseStorageQueueProcessor(this IWorkerApplicationBuilder app, ILoggerFactory loggerFactory, IAzureQueueClientConfiguration configuration, Func<WorkerApplicationOperation, String, IWorkerApplicationMiddlewareExecutionController, Task> queueMessageProcessor) 
		{
			// generate a logger 
			var logger = loggerFactory.CreateLogger("StorageQueueMiddleware");
			
			// create a storage account 
			var storageCredentials = new StorageCredentials(configuration.StorageAccountName, configuration.StorageAccountSecret);
            var azureStorageAccount = new CloudStorageAccount(storageCredentials, configuration.StorageAccountEndpointSuffix, true);

			// create the queue if not exists
			logger.LogInformation("Creating storage queue {0} in {1} if missing", configuration.StorageQueueName, configuration.StorageAccountName);
			azureStorageAccount.CreateCloudQueueClient()
				.GetQueueReference(configuration.StorageQueueName)
				.CreateIfNotExistsAsync()
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			// some state of our function 
			bool processedJobSinceLastMessage = true;
			
			// register our middleware		
			logger.LogInformation("Registering StorageQueueMiddleware");
			return app.Use(async (operation, next) =>
        	{
				// generate the queue client 
				var queueClient = azureStorageAccount.CreateCloudQueueClient();
			
				// initialize a queue reference 
				var taskQueue = queueClient.GetQueueReference(configuration.StorageQueueName);

				// check the next queue task
				if (processedJobSinceLastMessage)
				{
					logger.LogInformation("Waiting for new messages to process...");
					processedJobSinceLastMessage = false;
				}
				
				CloudQueueMessage retrievedMessage = await taskQueue.GetMessageAsync();
				if (retrievedMessage != null)
				{
					// log
					logger.LogInformation("Received new message from queue ({0} bytes)", retrievedMessage.AsBytes.Length);
					
					// set the state
					processedJobSinceLastMessage = true;
					
					// delete the received message 
					await taskQueue.DeleteMessageAsync(retrievedMessage);

					// process the next message as task    
					logger.LogInformation("Processing message...");
					await queueMessageProcessor(operation, retrievedMessage.AsString, next);
					
					// log
					logger.LogInformation("Processing message finished...");																			
				}
				else
				{
					// Let the next middleware finish our job						
					await next.Skip();
				}
        	});   
		}
	}
}
