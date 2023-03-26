using System;
using System.Threading.Tasks;
using CoreHelpers.Azure.Worker.Hosting;
using CoreHelpers.Azure.Worker.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using CoreHelpers.Azure.Worker.Clients;
using System.Collections.Generic;
using System.Linq;

namespace CoreHelpers.Azure.Worker.AppServices
{
	public static class AppServiceStorageQueueMiddlewareExtension	
	{
        private static async Task<IDictionary<IAzureQueueClientPriorityConfiguration, CloudStorageAccount>> InitializeQueues(IEnumerable<IAzureQueueClientPriorityConfiguration> queues, ILogger logger)
        {
            logger.LogInformation("Initializing queues");
            var result = new Dictionary<IAzureQueueClientPriorityConfiguration, CloudStorageAccount>();

            foreach (var queue in queues)
            {
                // some log
                logger.LogInformation($"Initializing {queue.StorageQueueName} in {queue.StorageAccountName} with priority {queue.Priority}");

                // create a storage account 
                var storageCredentials = new StorageCredentials(queue.StorageAccountName, queue.StorageAccountSecret);
                var azureStorageAccount = new CloudStorageAccount(storageCredentials, true);

                // create the queue if not exists
                logger.LogInformation("Creating storage queue {0} in {1} if missing", queue.StorageQueueName, queue.StorageAccountName);
                await azureStorageAccount.CreateCloudQueueClient().GetQueueReference(queue.StorageQueueName).CreateIfNotExistsAsync();

                // attache values
                result.Add(queue, azureStorageAccount);
            }

            return result;
        }

        private static async Task<Tuple<CloudQueueMessage, IAzureQueueClientPriorityConfiguration>> GetMessageFromQueues(IDictionary<IAzureQueueClientPriorityConfiguration, CloudStorageAccount> queues)
        {
            // order the queues
            var orderedQueues = queues.Keys.OrderBy(q => q.Priority);

            // ask the queues
            foreach (var queue in orderedQueues)
            {
                // get the storage account information
                var azureStorageAccount = queues[queue];

                // generate the queue client 
                var queueClient = azureStorageAccount.CreateCloudQueueClient();

                // initialize a queue reference 
                var taskQueue = queueClient.GetQueueReference(queue.StorageQueueName);

                // retrieve the message
                CloudQueueMessage retrievedMessage = await taskQueue.GetMessageAsync();
                if (retrievedMessage == null)
                    continue;
                    
                // delete the received message 
                await taskQueue.DeleteMessageAsync(retrievedMessage);

                // done 
                return new Tuple<CloudQueueMessage, IAzureQueueClientPriorityConfiguration>(retrievedMessage, queue);
            }

            // finish it 
            return null;
        }

        public static IWorkerApplicationBuilder UseStorageQueueProcessor(this IWorkerApplicationBuilder app, ILoggerFactory loggerFactory, IEnumerable<IAzureQueueClientPriorityConfiguration> queuesConfiguration, Func<WorkerApplicationOperation, String, IWorkerApplicationMiddlewareExecutionController, Task> queueMessageProcessor)
        {
            return UseStorageQueueProcessorInternal(app, loggerFactory, queuesConfiguration, true, queueMessageProcessor);
        }

        public static IWorkerApplicationBuilder UseStorageQueueProcessorNotBlocking(this IWorkerApplicationBuilder app, ILoggerFactory loggerFactory, IEnumerable<IAzureQueueClientPriorityConfiguration> queuesConfiguration, Func<WorkerApplicationOperation, String, IWorkerApplicationMiddlewareExecutionController, Task> queueMessageProcessor)
        {
            return UseStorageQueueProcessorInternal(app, loggerFactory, queuesConfiguration, false, queueMessageProcessor);
        }

        private static IWorkerApplicationBuilder UseStorageQueueProcessorInternal(this IWorkerApplicationBuilder app, ILoggerFactory loggerFactory, IEnumerable<IAzureQueueClientPriorityConfiguration> queuesConfiguration, bool blockWhenQueueIsEmpty, Func<WorkerApplicationOperation, String, IWorkerApplicationMiddlewareExecutionController, Task> queueMessageProcessor)
        {
            Task.Run(async () =>
            {
                // generate a logger 
                var logger = loggerFactory.CreateLogger("StorageQueueMiddleware");

                // initialize the queues
                var initializedQueues = await InitializeQueues(queuesConfiguration, logger);

                // some state of our function 
                bool processedJobSinceLastMessage = true;

                // register our middleware		
                logger.LogInformation("Registering StorageQueueMiddleware");
                return app.Use(async (operation, next) =>
                {
                    // check the next queue task
                    if (processedJobSinceLastMessage)
                    {
                        logger.LogInformation("Waiting for new messages to process...");
                        processedJobSinceLastMessage = false;
                    }

                    // retrieve the message
                    var retrievedMessage = await GetMessageFromQueues(initializedQueues);
                    if (retrievedMessage != null)
                    {
                        // log
                        logger.LogInformation($"Received new message from queue {retrievedMessage.Item2.PriorityDisplayName} ({retrievedMessage.Item1.AsBytes.Length} bytes)");

                        // set the state
                        processedJobSinceLastMessage = true;

                        // process the next message as task    
                        logger.LogInformation("Processing message...");
                        await queueMessageProcessor(operation, retrievedMessage.Item1.AsString, next);

                        // log
                        logger.LogInformation("Processing message finished...");
                    }
                    else if (blockWhenQueueIsEmpty)
                    {
                        // the polling will be skip and we land at the queue check again
                        await next.Skip();                      
                    } else
                    {
                        logger.LogInformation("All queues are empty, hand over processing to next middleware");
                        await next.Invoke();
                    }
                });
            }).Wait();

            return app;
		}
	}
}
