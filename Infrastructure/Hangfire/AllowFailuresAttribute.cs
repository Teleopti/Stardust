using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.States;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class AllowFailuresFromArgument : JobFilterAttribute, IElectStateFilter
	{
		private static readonly ILog Logger = LogProvider.For<AllowFailuresFromArgument>();

		public void OnStateElection(ElectStateContext context)
		{
			var args = context.BackgroundJob.Job.Args.ToArray();
			var jobInfo = args.OfType<HangfireEventJob>().SingleOrDefault();
			var allowFailures = jobInfo?.AllowFailures ?? 0;

			var failedState = context.CandidateState as FailedState;
			if (allowFailures == 0)
				return;
			if (failedState == null)
				return;

			var recurringId = jobInfo.RecurringId();
			var key = "allow-failures:" + recurringId;
			var failures = getHashValue<int>(context, key, "Failures");

			failures = failures + 1;
			if (failures <= allowFailures)
			{
				context.CandidateState = newSucceededState();
				Logger.WarnException(
					$"Failed to process the job '{context.BackgroundJob.Id}', '{recurringId}': an exception occurred. Fail {failures} of {allowFailures} was supressed. Next attempt will be performed on next occurrence.",
					failedState.Exception);
			}

			setHashValue(context, key, "Failures", failures);
		}

		private static void setHashValue<T>(ElectStateContext context, string key, string field, T value)
		{
			var values = new Dictionary<string, string> {{field, value.ToString()}};
			context.Connection.SetRangeInHash(key, values);
		}

		private static T getHashValue<T>(ElectStateContext context, string key, string field)
		{
			var values = context.Connection.GetAllEntriesFromHash(key);
			if (values?.ContainsKey(field) ?? false)
				return (T) Convert.ChangeType(values[field], typeof(T));
			return default(T);
		}
		
		private SucceededState newSucceededState()
		{
			return typeof(SucceededState)
				.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
				.Single()
				.Invoke(new object[] {null, 0, 0}) as SucceededState;
		}
	}
}