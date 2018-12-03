using System;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[InstancePerLifetimeScope]
	public class WebScheduleStardustHandler : IHandleEvent<WebScheduleStardustEvent>, IRunOnStardust
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IJobResultRepository _jobResultRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(WebScheduleStardustHandler));

		public WebScheduleStardustHandler(FullScheduling fullScheduling, IJobResultRepository jobResultRepository)
		{
			_fullScheduling = fullScheduling;
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			logger.Info($"Web Scheduling started for PlanningPeriod {@event.PlanningPeriodId} and JobResultId is {@event.JobResultId}");
			try
			{
				var result = _fullScheduling.DoSchedulingAndDO(@event.PlanningPeriodId);
				
				SaveDetailToJobResult(@event, DetailLevel.Info, JsonConvert.SerializeObject(result), null);
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebScheduleStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			_jobResultRepository.AddDetailAndCheckSuccess(@event.JobResultId, new JobResultDetail(level, message, DateTime.UtcNow, exception), 1);
		}
	}
}