using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Backlog;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public class ProductionReplanHelper : IProductionReplanHelper
	{
		private readonly OutboundProductionPlanFactory _outboundProductionPlanFactory;
		private readonly OutboundScheduledResourcesProvider _scheduledResourcesProvider;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IForecastingTargetMerger _forecastingTargetMerger;
		private readonly ISkillDayRepository _skillDayRepository;

		public ProductionReplanHelper(OutboundProductionPlanFactory outboundProductionPlanFactory, IFetchAndFillSkillDays fetchAndFillSkillDays, ISkillDayRepository skillDayRepository, IForecastingTargetMerger forecastingTargetMerger, OutboundScheduledResourcesProvider scheduledResourcesProvider)
		{
			_outboundProductionPlanFactory = outboundProductionPlanFactory;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_skillDayRepository = skillDayRepository;
			_forecastingTargetMerger = forecastingTargetMerger;
			_scheduledResourcesProvider = scheduledResourcesProvider;
		}

        public void Replan(IOutboundCampaign campaign)
		{
			if (campaign == null) return;

			var incomingTask = getIncomingTaskFromCampaign(campaign);
			incomingTask.RecalculateDistribution();
			//persist productionPlan
			updateSkillDays(campaign.Skill, incomingTask, true);
		}

		private void updateSkillDays(ISkill skill, IncomingTask incomingTask, bool applyDefaultTemplate)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				ICollection<ISkillDay> skillDays = _fetchAndFillSkillDays.FindRange(incomingTask.SpanningPeriod, skill);
				var workload = skill.WorkloadCollection.First();
				var workLoadDays = new List<IWorkloadDay>();
				foreach (var skillDay in skillDays)
				{
					var workloadDay = skillDay.WorkloadDayCollection.First();
					if (applyDefaultTemplate)
					{
						var dayOfWeek = skillDay.CurrentDate.DayOfWeek;
						var template = workload.TemplateWeekCollection[(int)dayOfWeek];
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
						forecastingTarget.Tasks = incomingTask.GetTimeOnDate(dateOnly).TotalSeconds / incomingTask.AverageWorkTimePerItem.TotalSeconds;
						forecastingTarget.AverageTaskTime = incomingTask.AverageWorkTimePerItem;
					}

					forecastingTargets.Add(forecastingTarget);
				}
				_forecastingTargetMerger.Merge(forecastingTargets, workLoadDays);

				_skillDayRepository.AddRange(skillDays);
				uow.PersistAll();
			}	
		}

        private IncomingTask getIncomingTaskFromCampaign(IOutboundCampaign campaign)
		{
			var incomingTask = _outboundProductionPlanFactory.CreateAndMakeInitialPlan(campaign.SpanningPeriod, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

			foreach (var dateOnly in incomingTask.SpanningPeriod.DayCollection())
			{
				var manualTime = campaign.GetManualProductionPlan(dateOnly);
				if (manualTime.HasValue)
					incomingTask.SetTimeOnDate(dateOnly, manualTime.Value, PlannedTimeTypeEnum.Manual);
				var scheduled = _scheduledResourcesProvider.GetScheduledTimeOnDate(dateOnly, campaign.Skill);
				var forecasted = _scheduledResourcesProvider.GetForecastedTimeOnDate(dateOnly, campaign.Skill);
				if (scheduled != TimeSpan.Zero)
					incomingTask.SetTimeOnDate(dateOnly, scheduled, PlannedTimeTypeEnum.Scheduled);
				else if (forecasted != TimeSpan.Zero && !manualTime.HasValue)
					incomingTask.SetTimeOnDate(dateOnly, forecasted, PlannedTimeTypeEnum.Calculated);
			}

			return incomingTask;
		}
	}
}