using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class WebIntradayOptimizationStardustHandler : IntradayOptimizationEventBaseHandler, IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;

		public WebIntradayOptimizationStardustHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder, IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult, IGridlockManager gridlockManager,
			IFillStateHolderWithMaxSeatSkills fillStateHolderWithMaxSeatSkills, IJobResultRepository jobResultRepository)
			: base(
				intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeIntradayOptimizationResult,
				gridlockManager, fillStateHolderWithMaxSeatSkills)
		{
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public override void Handle(OptimizationWasOrdered @event)
		{
			try
			{
				base.Handle(@event);
				SaveDetailToJobResult(@event, DetailLevel.Info, "", null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(OptimizationWasOrdered @event, DetailLevel level, string message, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), @event.TotalEvents);
		}
	}
}