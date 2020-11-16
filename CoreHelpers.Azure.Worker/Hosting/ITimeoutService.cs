using System;
using System.Threading.Tasks;

namespace CoreHelpers.Azure.Worker.Hosting
{
    public enum TimoutServiceWaitResult
    {
        taskFinished,
        taskTimedout
    }

    public interface ITimeoutService
    {
        Task<TimoutServiceWaitResult> WaitForExecution(Task executionTask, TimeSpan executionTimeout);

        Task ResetExecutionTimeout();
    }
}
