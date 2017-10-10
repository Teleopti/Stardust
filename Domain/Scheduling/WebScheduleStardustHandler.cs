﻿using System;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WebScheduleStardustHandler : IHandleEvent<WebScheduleStardustEvent>, IRunOnStardust
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILowThreadPriorityScope _lowThreadPriorityScope;
		private static readonly ILog logger = LogManager.GetLogger(typeof(WebScheduleStardustHandler));

		public WebScheduleStardustHandler(FullScheduling fullScheduling, IEventPopulatingPublisher eventPublisher, IJobResultRepository jobResultRepository, ILowThreadPriorityScope lowThreadPriorityScope)
		{
			_fullScheduling = fullScheduling;
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
			_lowThreadPriorityScope = lowThreadPriorityScope;
		}

		[AsSystem]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			logger.Info($"Web Scheduling started for PlanningPeriod {@event.PlanningPeriodId} and JobResultId is {@event.JobResultId}");
			try
			{
				using (_lowThreadPriorityScope.OnThisThread())
				{
					var result = _fullScheduling.DoScheduling(@event.PlanningPeriodId);
					SaveDetailToJobResult(@event, DetailLevel.Info, JsonConvert.SerializeObject(result), null);
					_eventPublisher.Publish(new WebDayoffOptimizationStardustEvent(@event)
					{
						JobResultId = @event.JobResultId
					});
				}
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
			var jobResult = _jobResultRepository.Get(@event.JobResultId);
			jobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}
	}
}