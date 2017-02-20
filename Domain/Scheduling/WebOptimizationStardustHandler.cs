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

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(WebOptimizationStardustEvent @event)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webOptimizationJobResult = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebOptimization);
			try
			{
				var result = _scheduleOptimization.Execute(@event.PlanningPeriodId);
				webOptimizationJobResult.AddDetail(new JobResultDetail(DetailLevel.Info, JsonConvert.SerializeObject(result), DateTime.UtcNow, null));
			}
			catch (Exception e)
			{
				webOptimizationJobResult.AddDetail(new JobResultDetail(DetailLevel.Error, null, DateTime.UtcNow, e));
			}
		}
	}
}