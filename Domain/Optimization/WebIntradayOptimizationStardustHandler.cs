using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class WebIntradayOptimizationStardustHandler : IntradayOptimizationEventBaseHandler, IRunOnStardust, IHandleEvent<WebIntradayOptimizationStardustEvent>
	{
		private readonly IJobResultRepository _jobResultRepository;

		public WebIntradayOptimizationStardustHandler(IntradayOptimization intradayOptimization,
			Func<ISchedulerStateHolder> schedulerStateHolder, IFillSchedulerStateHolder fillSchedulerStateHolder,
			ISynchronizeIntradayOptimizationResult synchronizeIntradayOptimizationResult, IGridlockManager gridlockManager,
			IJobResultRepository jobResultRepository)
			: base(intradayOptimization, schedulerStateHolder, fillSchedulerStateHolder, synchronizeIntradayOptimizationResult, gridlockManager)
		{
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public virtual void Handle(WebIntradayOptimizationStardustEvent @event)
		{
			try
			{
				HandleEvent(@event.OptimizationWasOrdered);
				SaveDetailToJobResult(@event, DetailLevel.Info, "", null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebIntradayOptimizationStardustEvent @event, DetailLevel level,
			string message, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), @event.TotalEvents);
		}
	}
}