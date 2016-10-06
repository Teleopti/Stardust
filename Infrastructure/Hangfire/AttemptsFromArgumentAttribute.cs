using System.Linq;
using Hangfire.States;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	// inherits from a copy of the retry filter from hangfire
	// because the hangfire implementation is sealed
	public class AttemptsFromArgumentAttribute : AutomaticRetryAttribute
	{
		private readonly string _format;

		public AttemptsFromArgumentAttribute()
		{
		}

		public AttemptsFromArgumentAttribute(string format)
		{
			_format = format;
		}

		public override void OnStateElection(ElectStateContext context)
		{
			var args = context.BackgroundJob.Job.Args;
			var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
			var attempts = jobInfo?.Attempts ?? int.Parse(string.Format(_format, args));

			Attempts = attempts - 1;

			base.OnStateElection(context);
		}
	}
}