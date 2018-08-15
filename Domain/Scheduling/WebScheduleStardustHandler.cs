using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeSchedulingAndDO_76496)]
	[DisabledBy(Toggles.ResourcePlanner_MergeSchedulingAndDO_76496)]
	public class WebScheduleStardustHandler : IHandleEvent<SchedulingAndDayOffWasOrdered>, IRunOnStardust
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly FullSchedulingResult _fullSchedulingResult;
		private readonly SchedulingInformationProvider _schedulingInformationProvider;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		
		private static readonly ILog logger = LogManager.GetLogger(typeof(WebScheduleStardustHandler));

		public WebScheduleStardustHandler(FullScheduling fullScheduling, IEventPopulatingPublisher eventPublisher, IJobResultRepository jobResultRepository, Func<ISchedulerStateHolder> schedulerStateHolder, FullSchedulingResult fullSchedulingResult, SchedulingInformationProvider schedulingInformationProvider, ISchedulingOptionsProvider schedulingOptionsProvider, ICurrentUnitOfWork currentUnitOfWork)
		{
			_fullScheduling = fullScheduling;
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
			_schedulerStateHolder = schedulerStateHolder;
			_fullSchedulingResult = fullSchedulingResult;
			_schedulingInformationProvider = schedulingInformationProvider;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[AsSystem]
		public virtual void Handle(SchedulingAndDayOffWasOrdered @event)
		{
			logger.Info($"Web Scheduling started for PlanningPeriod {@event.PlanningPeriodId} and JobResultId is {@event.JobResultId}");
			try
			{
				_fullScheduling.DoScheduling(@event.PlanningPeriodId);
				var stateHolder = _schedulerStateHolder();
				var schedulingInformation = _schedulingInformationProvider.GetInfoFromPlanningPeriod(@event.PlanningPeriodId);
				var schedulingOptions = _schedulingOptionsProvider.Fetch(null);
				var result = CreateResult(stateHolder.SchedulingResultState.LoadedAgents, schedulingInformation.Period, schedulingInformation.PlanningGroup, schedulingOptions.UsePreferences);
				SaveDetailToJobResult(@event, DetailLevel.Info, JsonConvert.SerializeObject(result), null);
				_eventPublisher.Publish(new WebDayoffOptimizationStardustEvent(@event)
				{
					JobResultId = @event.JobResultId
				});
			}
			catch (Exception e)
			{
				SaveDetailToJobResult(@event, DetailLevel.Error, "", e);
				throw;
			}
		}
		
		[TestLog]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(IEnumerable<IPerson> fixedStaffPeople, DateOnlyPeriod period, IPlanningGroup planningGroup, bool usePreferences)
		{
			_currentUnitOfWork.Current().Reassociate(fixedStaffPeople);
			return _fullSchedulingResult.Execute(period, fixedStaffPeople, planningGroup, usePreferences);
		}

		[UnitOfWork]
		protected virtual void SaveDetailToJobResult(SchedulingAndDayOffWasOrdered @event, DetailLevel level, string message, Exception exception)
		{
			var jobResult = _jobResultRepository.Get(@event.JobResultId);
			jobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}
	}
}