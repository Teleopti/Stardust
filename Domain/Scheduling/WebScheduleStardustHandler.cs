using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WebScheduleStardustHandler : IHandleEvent<WebScheduleStardustEvent>, IRunOnStardust
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly FullScheduling _fullScheduling;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ISchedulingSourceScope _schedulingSourceScope;

		public WebScheduleStardustHandler(IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, FullScheduling fullScheduling, IEventPopulatingPublisher eventPublisher, IJobResultRepository jobResultRepository, ISchedulingSourceScope schedulingSourceScope)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_fullScheduling = fullScheduling;
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
			_schedulingSourceScope = schedulingSourceScope;
		}

		[AsSystem]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			try
			{
				using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
				{
					var schedulingInformation = GetInfoFromPlanningPeriod(@event.PlanningPeriodId);
					var result = _fullScheduling.DoScheduling(schedulingInformation.Period, schedulingInformation.PersonIds);
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
		protected virtual SchedulingInformation GetInfoFromPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			if (planningPeriod.PlanningGroup != null)
			{
				return new SchedulingInformation(period,
					_planningGroupStaffLoader.LoadPersonIds(period, planningPeriod.PlanningGroup));
			}
			var people = _planningGroupStaffLoader.Load(period, null);
			return new SchedulingInformation(period, people.AllPeople.Select(x => x.Id.Value).ToList());
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(WebScheduleStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			var jobResult = _jobResultRepository.Get(@event.JobResultId);
			jobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}

		protected class SchedulingInformation
		{
			public IList<Guid> PersonIds { get; }
			public DateOnlyPeriod Period { get; }

			public SchedulingInformation(DateOnlyPeriod period, IList<Guid> personIds)
			{
				PersonIds = personIds;
				Period = period;
			}
		}
	}
}