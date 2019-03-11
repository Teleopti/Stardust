using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public class FakeTimeSinceLastPing : ITimeSinceLastPing
	{
		private readonly IList<CustomStatusStep> _wasExecuted = new List<CustomStatusStep>();
		
		public TimeSpan Execute(CustomStatusStep step)
		{
			_wasExecuted.Add(step);
			return TimeSpan.Zero;
		}
		
		public bool WasExecuted(CustomStatusStep step)
		{
			return _wasExecuted.Contains(step);
		}
	}
}