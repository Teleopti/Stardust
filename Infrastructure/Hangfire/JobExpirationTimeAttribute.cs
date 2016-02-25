using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class JobExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public int JobExpirationTimeoutSeconds { get; set;  }

		public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromSeconds(JobExpirationTimeoutSeconds);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
		}
	}
}