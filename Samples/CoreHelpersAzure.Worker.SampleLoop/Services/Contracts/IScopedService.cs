using System;
namespace CoreHelper.Azure.Worker.SampleLoop.Services.Contracts
{
	public interface IScopedService
	{
		string InstanceId { get; }
	}
}
