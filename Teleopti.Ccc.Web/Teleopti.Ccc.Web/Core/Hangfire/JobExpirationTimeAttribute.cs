using System;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Teleopti.Ccc.Domain.MultipleConfig;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class JobExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			var configReader = (IConfigReader)JobActivator.Current.ActivateJob(typeof(IConfigReader));
			context.JobExpirationTimeout = TimeSpan.FromSeconds(int.Parse(configReader.AppSettings["HangfireJobExpirationSeconds"]));
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
		}
	}
}