using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
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
					_intradayOptimizationFromWeb.Execute(@event.PlanningPeriodId, true);
					SaveDetailToJobResult(@event, DetailLevel.Info, "", null);
				}
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(IntradayOptimizationOnStardustWasOrdered @event, DetailLevel level, string message, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), @event.TotalEvents);
		}

		/*
		//TODO
		private IBlockPreferenceProvider blockPreferenceProvider(Guid? planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId.Value);
			var planningGroup = planningPeriod.PlanningGroup;
			var blockPreferenceProvider = _blockPreferenceProviderUsingFiltersFactory.Create(planningGroup);
			return blockPreferenceProvider;
		}
		*/
	}
}