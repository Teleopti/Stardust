using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;


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

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignMapper outboundCampaignMapper, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundSkillCreator outboundSkillCreator, IActivityRepository activityRepository, 
			IOutboundSkillPersister outboundSkillPersister, ICreateOrUpdateSkillDays createOrUpdateSkillDays, IProductionReplanHelper productionReplanHelper, IOutboundPeriodMover outboundPeriodMover)
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
		}

		public CampaignViewModel Persist(CampaignForm form)
		{
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
				SpanningPeriod = new DateOnlyPeriod(form.StartDate, form.EndDate)
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

			_outboundCampaignRepository.Add(campaign);
			_createOrUpdateSkillDays.Create(campaign.Skill, campaign.SpanningPeriod, campaign.CampaignTasks(),
				campaign.AverageTaskHandlingTime(), campaign.WorkingHours);

			return _outboundCampaignViewModelMapper.Map(campaign);
		}

		private IActivity getActivity(ActivityViewModel selectedActivity)
		{
			IActivity activity;
			if (selectedActivity.Id != null)
			{
				activity = _activityRepository.LoadAll().First(x => x.Id.Equals(selectedActivity.Id));
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
				var oldCampaign = _outboundCampaignRepository.Load((Guid) campaignViewModel.Id);
				campaign = _outboundCampaignMapper.Map(campaignViewModel);

				if (oldCampaign.Name != campaign.Name)
				{
					campaign.Skill.Name = campaign.Name;
				}

				if (campaignViewModel.Activity.Id != null && oldCampaign.Skill.Activity.Id != campaignViewModel.Activity.Id)
				{
					campaign.Skill.Activity = getActivity(campaignViewModel.Activity);
				}

				if (isNeedReplan(oldCampaign, campaign)) _productionReplanHelper.Replan(campaign);

				if (isMovePeriod(oldCampaign, campaign)) _outboundPeriodMover.Move(campaign, oldCampaign.SpanningPeriod);
			}

			return campaign;
		}

		private bool isWorkingHoursUpdated(IDictionary<DayOfWeek, TimePeriod> oldWorkingHours, IDictionary<DayOfWeek, TimePeriod> newWorkingHours)
		{
			for (var weekDay = DayOfWeek.Sunday; weekDay <= DayOfWeek.Saturday; ++weekDay)
			{
				if (!oldWorkingHours.ContainsKey(weekDay) && !newWorkingHours.ContainsKey(weekDay)) continue;
				
				if ( (oldWorkingHours.ContainsKey(weekDay) && !newWorkingHours.ContainsKey(weekDay))
					|| (!oldWorkingHours.ContainsKey(weekDay) && newWorkingHours.ContainsKey(weekDay))
					|| !oldWorkingHours[weekDay].Equals(newWorkingHours[weekDay]))
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
	}
}