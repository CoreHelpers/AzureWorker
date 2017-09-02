using System;
using System.Collections.Generic;

namespace CoreHelpers.Azure.Worker.Builder
{
    public class WorkerApplicationOperation
    {
		public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }
}
