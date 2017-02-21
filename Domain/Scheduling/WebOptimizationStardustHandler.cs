using System;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WebOptimizationStardustHandler :
		IHandleEvent<WebOptimizationStardustEvent>,
		IRunOnStardust
	{
		private readonly IScheduleOptimization _scheduleOptimization;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public WebOptimizationStardustHandler(IScheduleOptimization scheduleOptimization, IPlanningPeriodRepository planningPeriodRepository)
		{
			_scheduleOptimization = scheduleOptimization;
			_planningPeriodRepository = planningPeriodRepository;
		}

		[AsSystem]
		public virtual void Handle(WebOptimizationStardustEvent @event)
		{
			try
			{
				var result = _scheduleOptimization.Execute(@event.PlanningPeriodId);
				SaveDetailToJobResult(@event, DetailLevel.Info, JsonConvert.SerializeObject(result), null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, null, e);
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebOptimizationStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webOptimizationJobResult = planningPeriod.JobResults.Single(x => x.Id.Value == @event.JobResultId);
			webOptimizationJobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}
	}
}