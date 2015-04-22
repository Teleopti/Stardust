using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly ISkillRepository _skillRepository;
		private readonly IOutboundCampaignMapper _outboundCampaignMapper;
		private readonly IOutboundCampaignViewModelMapper _outboundCampaignViewModelMapper;
		private readonly IUserTimeZone _userTimeZone;

		public OutboundCampaignPersister(IOutboundCampaignRepository outboundCampaignRepository, ISkillRepository skillRepository, 
			IOutboundCampaignMapper outboundCampaignMapper, IOutboundCampaignViewModelMapper outboundCampaignViewModelMapper, IUserTimeZone userTimeZone)
		{
			_outboundCampaignRepository = outboundCampaignRepository;
			_skillRepository = skillRepository;
			_outboundCampaignMapper = outboundCampaignMapper;
			_outboundCampaignViewModelMapper = outboundCampaignViewModelMapper;
			_userTimeZone = userTimeZone;
		}

		public CampaignViewModel Persist(string name)
		{
			var skills = _skillRepository.LoadAll();
			var campaign = new Campaign(name, skills.FirstOrDefault());
			_outboundCampaignRepository.Add(campaign);

			return _outboundCampaignViewModelMapper.Map(campaign);
		}

		public Campaign Persist(CampaignViewModel campaignViewModel)
		{
			var skills = _skillRepository.LoadAll();
			Campaign campaign = null;

			if (campaignViewModel.Id.HasValue)
			{
				campaign = _outboundCampaignMapper.Map(campaignViewModel);
			}

			if (campaign != null) campaign.Skill = skills.First(x => x.Id.Equals(campaignViewModel.Skills.First(t => t.IsSelected).Id));
			return campaign;
		}


		public CampaignWorkingPeriod Persist(CampaignWorkingPeriodForm form)
		{
			if (!form.CampaignId.HasValue) return null;
			var campaign = _outboundCampaignRepository.Get(form.CampaignId.Value);
			if (campaign == null) return null;

			var start = TimeZoneHelper.ConvertToUtc(new DateTime(1900, 1, 1, form.StartTime.Hours, form.StartTime.Minutes, form.StartTime.Seconds), _userTimeZone.TimeZone());
			var end = TimeZoneHelper.ConvertToUtc(new DateTime(1900, 1, 1, form.EndTime.Hours, form.EndTime.Minutes, form.EndTime.Seconds), _userTimeZone.TimeZone());

			var campaignWorkingPeriod = new CampaignWorkingPeriod
			{
				TimePeriod = new TimePeriod(new TimeSpan(start.Hour, start.Minute, start.Second), new TimeSpan(end.Hour, end.Minute, end.Second)),
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