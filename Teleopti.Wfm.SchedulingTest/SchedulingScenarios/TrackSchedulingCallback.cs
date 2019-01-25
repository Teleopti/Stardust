using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public class TrackSchedulingCallback : ISchedulingCallback
	{
		private readonly IList<SchedulingCallbackInfo> callbacks = new List<SchedulingCallbackInfo>();

		public void Scheduled(SchedulingCallbackInfo callbackInfo)
		{
			callbacks.Add(callbackInfo);
		}

		public bool IsCancelled => false;

		public int SuccessfulScheduling()
		{
			return callbacks.Count(x => x.WasSuccessful);
		}

		public int UnSuccessfulScheduling()
		{
			return callbacks.Count(x => !x.WasSuccessful);
		}
	}
}