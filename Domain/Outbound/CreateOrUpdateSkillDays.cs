using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Outbound
{
	public interface ICreateOrUpdateSkillDays
	{
		void Create(ISkill skill, DateOnlyPeriod campaignPeriod, int campaignTasks,
			TimeSpan averageTimeForHandlingTasks, IDictionary<DayOfWeek, TimePeriod> workingHours);

		void UpdateSkillDays(ISkill skill, IBacklogTask incomingTask);
	}

	public class CreateOrUpdateSkillDays : ICreateOrUpdateSkillDays
	{
		private readonly OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;
		private readonly ISkillDayRepository _skillDayRepository;

		public CreateOrUpdateSkillDays(OutboundProductionPlanFactory outboundProductionPlanFactory,
			IFetchAndFillSkillDays fetchAndFillSkillDays, IForecastingTargetMerger forecastingTargetMerger,
			ISkillDayRepository skillDayRepository)
		{
			_outboundProductionPlanFactory = outboundProductionPlanFactory;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_forecastingTargetMerger = forecastingTargetMerger;
			_skillDayRepository = skillDayRepository;
		}

		public void Create(ISkill skill, DateOnlyPeriod campaignPeriod, int campaignTasks,
			TimeSpan averageTimeForHandlingTasks, IDictionary<DayOfWeek, TimePeriod> workingHours)
		{
			var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaignPeriod, campaignTasks,
				averageTimeForHandlingTasks, workingHours);

			UpdateSkillDays(skill, incomingTask);
		}

		public void UpdateSkillDays(ISkill skill, IBacklogTask incomingTask)
		{
			ICollection<ISkillDay> skillDays = _fetchAndFillSkillDays.FindRange(incomingTask.SpanningPeriod, skill);
			var workload = skill.WorkloadCollection.First();
			var workLoadDays = new List<IWorkloadDay>();
			foreach (var skillDay in skillDays)
			{
				var workloadDay = skillDay.WorkloadDayCollection.First();
				if(workloadDay.TemplateReference.TemplateId == Guid.Empty)
				{
					var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
					var template = workload.TemplateWeekCollection[(int) dayOfWeek];
					workloadDay.ApplyTemplate(template, day => { }, day => { });
				}

				workLoadDays.Add(workloadDay);
			}

			var forecastingTargets = new List<IForecastingTarget>();
			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var isOpen = incomingTask.PlannedTimeTypeOnDate(dateOnly) != PlannedTimeTypeEnum.Closed;
				var forecastingTarget = new ForecastingTarget(dateOnly, new OpenForWork(isOpen, isOpen));
				if (isOpen)
				{
					forecastingTarget.Tasks = incomingTask.GetTimeOnDate(dateOnly).TotalSeconds/
					                          incomingTask.AverageWorkTimePerItem.TotalSeconds;
					forecastingTarget.AverageTaskTime = incomingTask.AverageWorkTimePerItem;
				}

				forecastingTargets.Add(forecastingTarget);
			}
			_forecastingTargetMerger.Merge(forecastingTargets, workLoadDays);
			_skillDayRepository.AddRange(skillDays);
		}
	}
}