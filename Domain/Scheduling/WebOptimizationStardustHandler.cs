using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WebOptimizationStardustHandler :
		IHandleEvent<WebOptimizationStardustEvent>,
		IRunOnStardust
	{
		private readonly IScheduleOptimization _scheduleOptimization;
		private readonly IJobResultRepository _jobResultRepository;

		public WebOptimizationStardustHandler(IScheduleOptimization scheduleOptimization, IJobResultRepository jobResultRepository)
		{
			_scheduleOptimization = scheduleOptimization;
			_jobResultRepository = jobResultRepository;
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
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebOptimizationStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			var webOptimizationJobResult = _jobResultRepository.Get(@event.JobResultId);
			webOptimizationJobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}
	}
}