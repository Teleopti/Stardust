using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	[InstancePerLifetimeScope]
	public class WebIntradayOptimizationStardustHandler : IRunOnStardust, IHandleEvent<IntradayOptimizationOnStardustWasOrdered>
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ISchedulingSourceScope _schedulingSourceScope;
		private readonly IntradayOptimizationFromWeb _intradayOptimizationFromWeb;

		public WebIntradayOptimizationStardustHandler(
			IJobResultRepository jobResultRepository,
			ISchedulingSourceScope schedulingSourceScope, 
			IntradayOptimizationFromWeb intradayOptimizationFromWeb)
		{
			_jobResultRepository = jobResultRepository;
			_schedulingSourceScope = schedulingSourceScope;
			_intradayOptimizationFromWeb = intradayOptimizationFromWeb;
		}

		[AsSystem]
		public virtual void Handle(IntradayOptimizationOnStardustWasOrdered @event)
		{
			try
			{
				using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				{
					_intradayOptimizationFromWeb.Execute(@event.PlanningPeriodId);
					SaveDetailToJobResult(@event, DetailLevel.Info, null);
				}
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(IntradayOptimizationOnStardustWasOrdered @event, DetailLevel level, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, string.Empty, DateTime.UtcNow, exception), 1);
		}
	}
}