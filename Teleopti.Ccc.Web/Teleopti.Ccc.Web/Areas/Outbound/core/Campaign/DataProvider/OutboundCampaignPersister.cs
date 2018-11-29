using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Ccc.Web.Core.Data;



namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	using Campaign = Domain.Outbound.Campaign;

	public class OutboundCampaignPersister  : IOutboundCampaignPersister
	{		
		private readonly IOutboundCampaignRepository _outboundCampaignRepository;
		private readonly IOutboundCampaignMapper _outboundCampaignMapper;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IOutboundSkillCreator _outboundSkillCreator;
		private readonly IActivityRepository _activityRepository;
		private readonly IOutboundSkillPersister _outboundSkillPersister;
		private readonly ICreateOrUpdateSkillDays _createOrUpdateSkillDays;
		private readonly IProductionReplanHelper _productionReplanHelper;
		private readonly IOutboundPeriodMover _outboundPeriodMover;
		private readonly IOutboundCampaignTaskManager _campaignTaskManager;
		private readonly ISkillRepository _skillRepository;
		private readonly IOutboundScheduledResourcesCacher _outboundScheduledResourcesCacher;

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignMapper outboundCampaignMapper, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundSkillCreator outboundSkillCreator, IActivityRepository activityRepository, 
			IOutboundSkillPersister outboundSkillPersister, ICreateOrUpdateSkillDays createOrUpdateSkillDays, IProductionReplanHelper productionReplanHelper, 
			IOutboundPeriodMover outboundPeriodMover, IOutboundCampaignTaskManager campaignTaskManager, ISkillRepository skillRepository, IOutboundScheduledResourcesCacher outboundScheduledResourcesCacher)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignMapper = outboundCampaignMapper;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_outboundSkillCreator = outboundSkillCreator;
			_activityRepository = activityRepository;
			_outboundSkillPersister = outboundSkillPersister;
			_createOrUpdateSkillDays = createOrUpdateSkillDays;
			_productionReplanHelper = productionReplanHelper;
			_outboundPeriodMover = outboundPeriodMover;
			_campaignTaskManager = campaignTaskManager;
			_skillRepository = skillRepository;
			_outboundScheduledResourcesCacher = outboundScheduledResourcesCacher;
		}

		public CampaignViewModel Persist(CampaignForm form)
		{
			var period = new DateOnlyPeriod(form.StartDate,form.EndDate);
			var campaign = new Campaign()
			{
				Name = form.Name,
				CallListLen = form.CallListLen,
				TargetRate = form.TargetRate,
				ConnectRate = form.ConnectRate,
				RightPartyConnectRate = form.RightPartyConnectRate,
				ConnectAverageHandlingTime = form.ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = form.RightPartyAverageHandlingTime,
				UnproductiveTime = form.UnproductiveTime,
				BelongsToPeriod = period
			};

			if (form.WorkingHours != null)
			{
				foreach (CampaignWorkingHour workingHour in form.WorkingHours)
				{
					campaign.WorkingHours.Add(workingHour.WeekDay, new TimePeriod(workingHour.StartTime, workingHour.EndTime));
				}
			}

			var activity = getActivity(form.Activity);
			var skill = _outboundSkillCreator.CreateSkill(activity, campaign);
			_outboundSkillPersister.PersistSkill(skill);
			campaign.Skill = skill;
			var spanningPeriod = period.ToDateTimePeriod(campaign.Skill.TimeZone).ChangeEndTime(TimeSpan.FromSeconds(-1));
			campaign.SpanningPeriod = spanningPeriod;

			_outboundCampaignRepository.Add(campaign);
			_createOrUpdateSkillDays.Create(campaign.Skill, period, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

			return _outboundCampaignViewModelMapper.Map(campaign);
		}

		private IActivity getActivity(ActivityViewModel selectedActivity)
		{
			IActivity activity;
			if (selectedActivity.Id.HasValue)
			{
				activity = _activityRepository.Get(selectedActivity.Id.Value);
			}
			else
			{
				activity = new Activity(selectedActivity.Name)
				{
					DisplayColor = Color.Black,
					InContractTime = true,
					InPaidTime = true,
					InWorkTime = true,
					RequiresSkill = true,
					IsOutboundActivity = true,
					AllowOverwrite = true
				};
				_activityRepository.Add(activity);
			}

			return activity;
		}

		public IOutboundCampaign Persist(CampaignViewModel campaignViewModel)
		{
			IOutboundCampaign campaign = null;

			if (campaignViewModel.Id.HasValue)
			{
				var oldCampaign = (IOutboundCampaign) _outboundCampaignRepository.Load(campaignViewModel.Id.GetValueOrDefault()).Clone();
				campaign = _outboundCampaignMapper.Map(campaignViewModel);

				if (oldCampaign.Name != campaign.Name)
				{
					campaign.Skill.ChangeName(campaign.Name);
				}

				if (campaignViewModel.Activity.Id == null || oldCampaign.Skill.Activity.Id != campaignViewModel.Activity.Id)
				{
					campaign.Skill.Activity = getActivity(campaignViewModel.Activity);
				}

				if (isNeedReplan(oldCampaign, campaign)) _productionReplanHelper.Replan(campaign);

				if (isMovePeriod(oldCampaign, campaign))
				{
					_outboundSkillCreator.SetOpenHours(campaign, campaign.Skill.WorkloadCollection.First());
					_outboundSkillCreator.SetHandledWithinOnSkillTemplates(campaign.Skill);
					_outboundPeriodMover.Move(campaign, oldCampaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone));					
				}
			}
			return campaign;
		}

		public void PersistActualBacklog(ActualBacklogForm actualBacklog)
		{
			var campaign = _outboundCampaignRepository.Get(actualBacklog.CampaignId);
			if (campaign == null) return;

			foreach (var backlog in actualBacklog.ActualBacklog)
			{
				var time = doubleToTimeSpan(backlog.Time);
				if (campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).Contains(backlog.Date))
				{
					campaign.SetActualBacklog(backlog.Date, time);					
				}
			}
		}

		public void RemoveActualBacklog(RemoveActualBacklogForm manualPlan)
		{
			var campaign = _outboundCampaignRepository.Get(manualPlan.CampaignId);
			if (campaign == null) return;

			foreach (var date in manualPlan.Dates)
			{
				campaign.ClearActualBacklog(date);
			}			
		}

		public void PersistManualProductionPlan(ManualPlanForm manualPlan)
		{
			var campaign = _outboundCampaignRepository.Get(manualPlan.CampaignId);
			if (campaign == null) return;

			var isUpdateForecasted = false;
			foreach (var manual in manualPlan.ManualProductionPlan)
			{				
				var time = doubleToTimeSpan(manual.Time);
				if (campaign.SpanningPeriod.ToDateOnlyPeriod(campaign.Skill.TimeZone).Contains(manual.Date))
				{
					campaign.SetManualProductionPlan(manual.Date, time);
					isUpdateForecasted = true;
				}
			}

			if (isUpdateForecasted)
			{
				var incomingTask = manualPlan.SkipDates != null ? _campaignTaskManager.GetIncomingTaskFromCampaign(campaign, manualPlan.SkipDates.ToList()) 
																: _campaignTaskManager.GetIncomingTaskFromCampaign(campaign);

				var forecasts = _createOrUpdateSkillDays.UpdateSkillDays(campaign.Skill, incomingTask);				
				_outboundScheduledResourcesCacher.SetForecastedTime(campaign, forecasts);
			}
		}

		public void RemoveManualProductionPlan(RemoveManualPlanForm manualPlan)
		{
			var campaign = _outboundCampaignRepository.Get(manualPlan.CampaignId);
			if (campaign == null) return;

			foreach (var date in manualPlan.Dates)
			{
				campaign.ClearProductionPlan(date);
			}

			_productionReplanHelper.Replan(campaign, manualPlan.SkipDates.ToArray());
		}

		public void ManualReplanCampaign(PlanWithScheduleForm planForm)
		{
			var campaign = _outboundCampaignRepository.Get(planForm.CampaignId);
			_productionReplanHelper.Replan(campaign, planForm.SkipDates.ToArray());
		}

		public void RemoveCampaign(IOutboundCampaign campaign)
		{
			var shouldRemoveActivity = true;
			var activity = _activityRepository.Get(campaign.Skill.Activity.Id.GetValueOrDefault());
			if (activity != null)
			{
				if (activity.IsOutboundActivity)
				{
					var skills = _skillRepository.LoadAll();
					foreach (var skill in skills)
					{
						if (skill.Activity.Equals(activity) && !skill.Equals(campaign.Skill))
						{
							shouldRemoveActivity = false;
							break;
						}
					}
				}
				else
				{
					shouldRemoveActivity = false;
				}
			}

			if (shouldRemoveActivity) _activityRepository.Remove(activity);
			_outboundSkillPersister.RemoveSkill(campaign.Skill);
			_outboundCampaignRepository.Remove(campaign);
		}

		private bool isWorkingHoursUpdated(IDictionary<DayOfWeek, TimePeriod> oldWorkingHours, IDictionary<DayOfWeek, TimePeriod> newWorkingHours)
		{
			for (var weekDay = DayOfWeek.Sunday; weekDay <= DayOfWeek.Saturday; ++weekDay)
			{
				TimePeriod oldWorkingHoursValue, newWorkingHoursValue;
				var gotOld = oldWorkingHours.TryGetValue(weekDay, out oldWorkingHoursValue);
				var gotNew = newWorkingHours.TryGetValue(weekDay, out newWorkingHoursValue);

				if (gotOld == false && gotNew == false) continue;
				if (!oldWorkingHoursValue.Equals(newWorkingHoursValue) || gotOld ^ gotNew)
				{
					return true;
				}
			}
			return false;
		}

		private bool isMovePeriod(IOutboundCampaign oldCampaign, IOutboundCampaign campaign)
		{
			if (!oldCampaign.SpanningPeriod.Equals(campaign.SpanningPeriod)
			    || isWorkingHoursUpdated(oldCampaign.WorkingHours, campaign.WorkingHours)) return true;

			return false;
		}

		private bool isNeedReplan(IOutboundCampaign oldCampaign, IOutboundCampaign campaign)
		{
				if (oldCampaign.CallListLen != campaign.CallListLen
				    || oldCampaign.TargetRate != campaign.TargetRate
					|| oldCampaign.ConnectRate != campaign.ConnectRate
					|| oldCampaign.RightPartyConnectRate != campaign.RightPartyConnectRate
					|| oldCampaign.ConnectAverageHandlingTime != campaign.ConnectAverageHandlingTime
					|| oldCampaign.RightPartyAverageHandlingTime != campaign.RightPartyAverageHandlingTime
					|| oldCampaign.UnproductiveTime != campaign.UnproductiveTime)
				{
				return true;
				}

			return false;
		}

		private TimeSpan doubleToTimeSpan(double x)
		{
			var days = (int)x / 24;
			var hours = (int)(x - days * 24);
			var minutes = (int)((x - days * 24 - hours) * 60);
			var seconds = (int)((x - days * 24 - hours - (double)minutes / 60) * 60 * 60);
			return new TimeSpan(days, hours, minutes, seconds);
		}
	}
}