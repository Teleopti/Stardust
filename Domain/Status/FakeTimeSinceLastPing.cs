using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Status
{
	public class FakeTimeSinceLastPing : ITimeSinceLastPing
	{
		private readonly IList<CustomStatusStep> _wasExecuted = new List<CustomStatusStep>();
		private TimeSpan _timeSpan;

		public FakeTimeSinceLastPing SetValue(TimeSpan timeSpan)
		{
			_timeSpan = timeSpan;
			return this;
		}
		
		public TimeSpan Execute(CustomStatusStep step)
		{
			_wasExecuted.Add(step);
			return _timeSpan;
		}
		
		public bool WasExecuted(CustomStatusStep step)
		{
			return _wasExecuted.Contains(step);
		}
	}
}