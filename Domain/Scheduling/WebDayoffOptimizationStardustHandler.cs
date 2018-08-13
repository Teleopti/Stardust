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
	public class WebDayoffOptimizationStardustHandler :
		IHandleEvent<WebDayoffOptimizationStardustEvent>,
		IRunOnStardust
	{
		private readonly DayOffOptimizationWeb _dayOffOptimizationWeb;
		private readonly IJobResultRepository _jobResultRepository;

		public WebDayoffOptimizationStardustHandler(DayOffOptimizationWeb dayOffOptimizationWeb, IJobResultRepository jobResultRepository)
		{
			_dayOffOptimizationWeb = dayOffOptimizationWeb;
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public virtual void Handle(WebDayoffOptimizationStardustEvent @event)
		{
			try
			{
				var result = _dayOffOptimizationWeb.Execute(@event.PlanningPeriodId);
				SaveDetailToJobResult(@event, DetailLevel.Info, JsonConvert.SerializeObject(result), null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebDayoffOptimizationStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			// expected success is 2 here, because one is scheduling, the other is dayoff optimization
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), 2);
		}
	}
}