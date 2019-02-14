using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MonitorSystem
{
	public class MonitorResult
	{
		public MonitorResult(bool success, IEnumerable<string> outputs)
		{
			Success = success;
			Outputs = outputs;
		}

		public bool Success { get; }
		public IEnumerable<string> Outputs { get; }
	}
}