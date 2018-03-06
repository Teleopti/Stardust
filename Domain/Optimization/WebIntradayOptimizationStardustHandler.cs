using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class WebIntradayOptimizationStardustHandler : IRunOnStardust, IHandleEvent<WebIntradayOptimizationStardustEvent>
	{
		private readonly IntradayOptimizationExecutor _intradayOptimizationExecutor;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ISchedulingSourceScope _schedulingSourceScope;
		private readonly ILowThreadPriorityScope _lowThreadPriorityScope;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;

		public WebIntradayOptimizationStardustHandler(
			IntradayOptimizationExecutor intradayOptimizationExecutor,
			IJobResultRepository jobResultRepository,
			ISchedulingSourceScope schedulingSourceScope, 
			ILowThreadPriorityScope lowThreadPriorityScope,
			IPlanningPeriodRepository planningPeriodRepository, 
			BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory)
		{
			_intradayOptimizationExecutor = intradayOptimizationExecutor;
			_jobResultRepository = jobResultRepository;
			_schedulingSourceScope = schedulingSourceScope;
			_lowThreadPriorityScope = lowThreadPriorityScope;
			_planningPeriodRepository = planningPeriodRepository;
			_blockPreferenceProviderUsingFiltersFactory = blockPreferenceProviderUsingFiltersFactory;
		}

		//TODO: needed to remark [AsSystem] for now to be able to run test
		//if something is broken "for real", write a test for this and fix it!
		//[AsSystem]
		public virtual void Handle(WebIntradayOptimizationStardustEvent @event)
		{
			try
			{
				using (_lowThreadPriorityScope.OnThisThread())
				using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				{
					_intradayOptimizationExecutor.HandleEvent(blockPreferenceProvider(@event.PlanningPeriodId), @event.IntradayOptimizationWasOrdered, @event.PlanningPeriodId);
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
		protected virtual void SaveDetailToJobResult(WebIntradayOptimizationStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), @event.TotalEvents);
		}

		[UnitOfWork]
		protected virtual IBlockPreferenceProvider blockPreferenceProvider(Guid? planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId.Value);
			var planningGroup = planningPeriod.PlanningGroup;
			var blockPreferenceProvider = _blockPreferenceProviderUsingFiltersFactory.Create(planningGroup);
			return blockPreferenceProvider;
		}
	}
}