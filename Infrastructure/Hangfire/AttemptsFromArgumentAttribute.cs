using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	// inherits from a copy of the retry filter from hangfire
	// because the hangfire implementation is sealed
	//public class AttemptsFromArgumentAttribute : AutomaticRetryAttribute
	//{
	//	private readonly string _format;

	//	public AttemptsFromArgumentAttribute()
	//	{
	//	}

	//	public AttemptsFromArgumentAttribute(string format)
	//	{
	//		_format = format;
	//	}

	//	public override void OnStateElection(ElectStateContext context)
	//	{
	//		var args = context.BackgroundJob.Job.Args.ToArray();
	//		var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
	//		var attempts = jobInfo?.Attempts ?? int.Parse(string.Format(_format, args));

	//		Attempts = attempts - 1;
			
	//		base.OnStateElection(context);
	//	}
	//}

	public class AttemptsFromArgumentAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter
	{
		private readonly string _format;

		public AttemptsFromArgumentAttribute()
		{
		}

		public AttemptsFromArgumentAttribute(string format)
		{
			_format = format;
		}

		public void OnStateElection(ElectStateContext context)
		{
			var args = context.BackgroundJob.Job.Args.ToArray();
			var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
			var attempts = (jobInfo?.Attempts ?? int.Parse(string.Format(_format, args))) -1;
			var original = new AutomaticRetryAttribute {Attempts = attempts};
			original.OnStateElection(context);
		}

		public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			new AutomaticRetryAttribute().OnStateApplied(context, transaction);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			new AutomaticRetryAttribute().OnStateUnapplied(context, transaction);
		}
	}
}