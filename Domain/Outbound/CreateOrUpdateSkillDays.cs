using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Outbound
{
	public interface ICreateOrUpdateSkillDays
	{
		void Create(ISkill skill, DateOnlyPeriod campaignPeriod, int campaignTasks,
			TimeSpan averageTimeForHandlingTasks, IDictionary<DayOfWeek, TimePeriod> workingHours);
	
		Dictionary<DateOnly, TimeSpan> UpdateSkillDays(ISkill skill, IBacklogTask incomingTask);
	}

	public class CreateOrUpdateSkillDays : ICreateOrUpdateSkillDays
	{
		private readonly OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IOutboundScheduledResourcesProvider _outboundScheduledResourcesProvider;

		public CreateOrUpdateSkillDays(OutboundProductionPlanFactory outboundProductionPlanFactory,
			IFetchAndFillSkillDays fetchAndFillSkillDays, IForecastingTargetMerger forecastingTargetMerger,
			ISkillDayRepository skillDayRepository, IOutboundScheduledResourcesProvider outboundScheduledResourcesProvider)
		{
			_outboundProductionPlanFactory = outboundProductionPlanFactory;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_forecastingTargetMerger = forecastingTargetMerger;
			_skillDayRepository = skillDayRepository;
			_outboundScheduledResourcesProvider = outboundScheduledResourcesProvider;
		}

		public void Create(ISkill skill, DateOnlyPeriod campaignPeriod, int campaignTasks,
			TimeSpan averageTimeForHandlingTasks, IDictionary<DayOfWeek, TimePeriod> workingHours)
		{
			var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaignPeriod, campaignTasks,
				averageTimeForHandlingTasks, workingHours);

			UpdateSkillDays(skill, incomingTask);
		}

		
		public Dictionary<DateOnly, TimeSpan> UpdateSkillDays(ISkill skill, IBacklogTask incomingTask)
		{
			ICollection<ISkillDay> skillDays = _fetchAndFillSkillDays.FindRange(incomingTask.SpanningPeriod, skill);
			var workload = skill.WorkloadCollection.First();
			var workLoadDays = new List<IWorkloadDay>();
			foreach (var skillDay in skillDays)
			{
				var workloadDay = skillDay.WorkloadDayCollection.First();
				if (workloadDay.TemplateReference.TemplateId == Guid.Empty)
				{
					var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
					var template = workload.TemplateWeekCollection[(int) dayOfWeek];
					workloadDay.ApplyTemplate(template, day => { }, day => { });
				}

				workLoadDays.Add(workloadDay);
			}

			var forecastingTargets = new List<IForecastingTarget>();
			var forecasts = new Dictionary<DateOnly, TimeSpan>();

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var isOpen = incomingTask.PlannedTimeTypeOnDate(dateOnly) != PlannedTimeTypeEnum.Closed;
				var forecastingTarget = new ForecastingTarget(dateOnly, new OpenForWork(isOpen, isOpen));
				if (isOpen)
				{
					var timeOnDate = incomingTask.GetTimeOnDate(dateOnly);
					forecastingTarget.Tasks = timeOnDate.TotalSeconds / incomingTask.AverageWorkTimePerItem.TotalSeconds;
					forecastingTarget.AverageTaskTime = incomingTask.AverageWorkTimePerItem;
					_outboundScheduledResourcesProvider.SetForecastedTimeOnDate(dateOnly, skill, timeOnDate);
					
					forecasts.Add(dateOnly, timeOnDate);					
				}

				forecastingTargets.Add(forecastingTarget);
			}
			_forecastingTargetMerger.Merge(forecastingTargets, workLoadDays);
			foreach (var skillDay in skillDays)
			{
				var workloadDay = skillDay.WorkloadDayCollection.First();
				var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
				var template = workload.TemplateWeekCollection[(int)dayOfWeek];
				workloadDay.ApplyTemplate(template, day => { }, day => { });
			}
			_skillDayRepository.AddRange(skillDays);
			return forecasts;
		}
	}
}