using System;
using CoreHelper.Azure.Worker.SampleLoop.Services.Contracts;

namespace CoreHelper.Azure.Worker.SampleLoop.Services
{
	public class ScopedServiceImp : IScopedService
	{
		private Guid InstanceUUID { get; set; } = Guid.NewGuid();

		string IScopedService.InstanceId => InstanceUUID.ToString();
	}
}
