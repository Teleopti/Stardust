using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ClearScheduleStardustHandler : IHandleEvent<ClearScheduleStardustEvent>, IRunOnStardust
	{
		private readonly ClearPlanningPeriodSchedule _clearPlanningPeriodSchedule;
		private readonly IJobResultRepository _jobResultRepository;
		
		private static readonly ILog logger = LogManager.GetLogger(typeof(WebScheduleStardustHandler));

		public ClearScheduleStardustHandler(ClearPlanningPeriodSchedule clearPlanningPeriodSchedule, IJobResultRepository jobResultRepository)
		{
			_clearPlanningPeriodSchedule = clearPlanningPeriodSchedule;
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public virtual void Handle(ClearScheduleStardustEvent @event)
		{
			logger.Info(
				$"Web Clear Schedule started for PlanningPeriod {@event.PlanningPeriodId} and JobResultId is {@event.JobResultId}");
			try
			{
				_clearPlanningPeriodSchedule.ClearSchedules(@event.PlanningPeriodId);
				SaveDetailToJobResult(@event, DetailLevel.Info, null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(ClearScheduleStardustEvent @event, DetailLevel level, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, string.Empty, DateTime.UtcNow, exception), 1);
		}
	}
}