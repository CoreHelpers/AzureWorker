using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public class TimeoutService : ITimeoutService
    {
        private CancellationTokenSource cancellationTokenForWait { get; set; }
        private ILogger logger { get; set; }

        public TimeoutService(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger("TimeoutService");
        }

        public async Task ResetExecutionTimeout()
        {
            await Task.CompletedTask;
            this.logger.LogInformation("Reset Execution-Timeout triggered");
            cancellationTokenForWait.Cancel();            
        }

        public async Task<TimoutServiceWaitResult> WaitForExecution(Task executionTask, TimeSpan executionTimeout)
        {
            // make the compiler happy
            await Task.CompletedTask;
        
            // build the cancelation token
            cancellationTokenForWait = new CancellationTokenSource();
            
            // build the wait queue
            var tasksToWait = new List<Task>();
            tasksToWait.Add(executionTask);

            // build the timeout task                 
            var timeoutTask = default(Task);
            if (executionTimeout > TimeSpan.Zero)
            {
                timeoutTask = Task.Delay(executionTimeout);
                tasksToWait.Add(timeoutTask);
            }

            // wait for all
            try
            {
                var waitResult = Task.WaitAny(tasksToWait.ToArray(), cancellationTokenForWait.Token);
                if (tasksToWait[waitResult] == timeoutTask)
                    return TimoutServiceWaitResult.taskTimedout;
                else
                    return TimoutServiceWaitResult.taskFinished;
            } catch(OperationCanceledException)
            {
                this.logger.LogInformation("Reset Execution-Timeout executed");
                return await WaitForExecution(executionTask, executionTimeout);
            }
        }
    }
}
