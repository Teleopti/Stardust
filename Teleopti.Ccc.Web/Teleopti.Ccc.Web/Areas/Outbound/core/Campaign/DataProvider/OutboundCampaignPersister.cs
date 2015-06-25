using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NHibernate;
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

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, IOutboundCampaignMapper outboundCampaignMapper, 
			IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IOutboundSkillCreator outboundSkillCreator, IActivityRepository activityRepository, 
			IOutboundSkillPersister outboundSkillPersister, ICreateOrUpdateSkillDays createOrUpdateSkillDays)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_outboundCampaignMapper = outboundCampaignMapper;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_outboundSkillCreator = outboundSkillCreator;
			_activityRepository = activityRepository;
			_outboundSkillPersister = outboundSkillPersister;
			_createOrUpdateSkillDays = createOrUpdateSkillDays;
		}

		public CampaignViewModel Persist(string name)  //should take an activity as well
		{
			var campaign = new Campaign(){Name = name};
			var activity = _activityRepository.LoadAll().First();
			var skill = _outboundSkillCreator.CreateSkill(activity, campaign);
			_outboundSkillPersister.PersistSkill(skill);
			
			campaign.Skill = skill;
			_outboundCampaignRepository.Add(campaign);

			return _outboundCampaignViewModelMapper.Map(campaign);
		}

		public CampaignViewModel Persist(CampaignForm form)
		{
			var campaign = new Campaign()
			{
				Name = form.Name,
				CallListLen = form.CallListLen,
				ConnectRate = form.ConnectRate,
				RightPartyConnectRate = form.RightPartyConnectRate,
				ConnectAverageHandlingTime = form.ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = form.RightPartyAverageHandlingTime,
				UnproductiveTime = form.UnproductiveTime,
				SpanningPeriod = new DateOnlyPeriod(form.StartDate, form.EndDate)
			};

			campaign.TargetRate = form.CallListLen * form.ConnectRate / 100;
			if (form.WorkingHours != null)
			{
				foreach (CampaignWorkingHour workingHour in form.WorkingHours)
				{
					campaign.WorkingHours.Add(workingHour.WeekDay, workingHour.WorkingPeriod);
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
			var activity = _activityRepository.LoadAll().First();
			if (selectedActivity == null) return activity;
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

		public Campaign Persist(CampaignViewModel campaignViewModel)
		{
			Campaign campaign = null;

			if (campaignViewModel.Id.HasValue)
			{
				campaign = _outboundCampaignMapper.Map(campaignViewModel);
			}

			return campaign;
		}

		public CampaignWorkingPeriod Persist(CampaignWorkingPeriodForm form)
		{
			if (!form.CampaignId.HasValue) return null;
			var campaign = _outboundCampaignRepository.Get(form.CampaignId.Value);
			if (campaign == null) return null;

			var campaignWorkingPeriod = new CampaignWorkingPeriod
			{
				TimePeriod = new TimePeriod(form.StartTime, form.EndTime),
				CampaignWorkingPeriodAssignments = new HashSet<CampaignWorkingPeriodAssignment>()
			};

			NHibernateUtil.Initialize(campaign);
			campaign.AddWorkingPeriod(campaignWorkingPeriod);
			return campaignWorkingPeriod;
		}

		public Campaign Persist(CampaignWorkingPeriodAssignmentForm form)
		{
			if (!form.CampaignId.HasValue) return null;
			var campaign = _outboundCampaignRepository.Get(form.CampaignId.Value);
			if (campaign == null) return null;

			var specifiedWorkingPeriods = campaign.CampaignWorkingPeriods.Where(workingPeriod =>
			{
				return form.CampaignWorkingPeriods.Any(inputWorkingPeriodId => inputWorkingPeriodId == workingPeriod.Id);
			}).ToList();

                        var unspecifiedWorkingPeriods = campaign.CampaignWorkingPeriods.Except(specifiedWorkingPeriods).ToList();



			foreach (var p in specifiedWorkingPeriods)
			{
				if (p.CampaignWorkingPeriodAssignments.Any(a => a.WeekdayIndex == form.WeekDay)) continue;
				var assignment = new CampaignWorkingPeriodAssignment { WeekdayIndex = form.WeekDay };
				p.AddAssignment(assignment);
			}

			foreach (var p in unspecifiedWorkingPeriods)
			{
				var assignments = p.CampaignWorkingPeriodAssignments.Where(a => a.WeekdayIndex == form.WeekDay).ToList();
				foreach (var a in assignments)
				{
					p.RemoveAssignment(a);
				}
			}
			
			return campaign;
		}

	}
}