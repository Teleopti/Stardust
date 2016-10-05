using System.Linq;
using Hangfire.States;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	// inherits from a copy of the retry filter from hangfire
	// because the hangfire implementation is sealed
	public class AttemptsFromAttribute : AutomaticRetryAttribute
	{
		private readonly string _attemptCount;

		public AttemptsFromAttribute(string attemptCount)
		{
			_attemptCount = attemptCount;
		}

		public override void OnStateElection(ElectStateContext context)
		{
			var args = context.BackgroundJob.Job.Args.ToArray();
			var attempts = int.Parse(string.Format(_attemptCount, args));
			Attempts = attempts - 1;

			base.OnStateElection(context);
		}
	}
}