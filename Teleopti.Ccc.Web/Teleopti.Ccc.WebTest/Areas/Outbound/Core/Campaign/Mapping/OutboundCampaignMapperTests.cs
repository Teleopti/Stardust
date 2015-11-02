using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignMapperTests
	{
		private IOutboundCampaignRepository _outboundCampaignRepository;
		private CampaignViewModel _campaignViewModel;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = new FakeUserTimeZone(TimeZoneInfo.Utc);
			_outboundCampaignRepository = MockRepository.GenerateMock<IOutboundCampaignRepository>();
			_campaignViewModel = new CampaignViewModel() { Id = new Guid() };
		}

		[Test]
		public void ShouldMapNothingWhenNoCampaignFind()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).IgnoreArguments().Return(null);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldMapName()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.Name = "myCampaign";

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.Name.Should().Be.EqualTo(_campaignViewModel.Name);
		}		
		
		[Test]
		public void ShouldMapCallListLen()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.CallListLen = 8;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.CallListLen.Should().Be.EqualTo(_campaignViewModel.CallListLen);
		}		
		
		[Test]
		public void ShouldMapTargetRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.TargetRate = 18;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.TargetRate.Should().Be.EqualTo(_campaignViewModel.TargetRate);
		}		
		
		[Test]
		public void ShouldMapConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.ConnectRate = 28;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.ConnectRate.Should().Be.EqualTo(_campaignViewModel.ConnectRate);
		}		
		
		[Test]
		public void ShouldMapRightPartyAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.RightPartyAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaignViewModel.RightPartyAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapConnectAverageHandlingTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.ConnectAverageHandlingTime = 38;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.ConnectAverageHandlingTime.Should().Be.EqualTo(_campaignViewModel.ConnectAverageHandlingTime);
		}		
		
		[Test]
		public void ShouldMapRightPartyConnectRate()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.RightPartyConnectRate = 48;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.RightPartyConnectRate.Should().Be.EqualTo(_campaignViewModel.RightPartyConnectRate);
		}
		
		[Test]
		public void ShouldMapUnproductiveTime()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.UnproductiveTime = 58;

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.UnproductiveTime.Should().Be.EqualTo(_campaignViewModel.UnproductiveTime);
		}		
		
		[Test]
		public void ShouldMapStartDate()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = SkillFactory.CreateSkill("mySkill");
			campaign.Skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(campaign);
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.EndDate = new DateOnly(2015, 4, 20);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.SpanningPeriod.StartDateTime.Day.Should().Be.EqualTo(12);
		}

		[Test]
		public void ShouldMapEndDate()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.Skill = SkillFactory.CreateSkill("mySkill");
			campaign.Skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.EndDate = new DateOnly(2015, 4, 20);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.SpanningPeriod.EndDateTime.Day.Should().Be.EqualTo(20);
		}

		[Test]
		public void ShouldMapBelongsToPeriod()
		{
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(new Domain.Outbound.Campaign());
			_campaignViewModel.StartDate = new DateOnly(2015, 4, 13);
			_campaignViewModel.EndDate = new DateOnly(2015, 4, 20);

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.BelongsToPeriod.Should().Be.EqualTo(new DateOnlyPeriod(_campaignViewModel.StartDate, _campaignViewModel.EndDate));
		}
		
		[Test]
		public void ShouldMapWorkingHours()
		{
			var campaign = new Domain.Outbound.Campaign();
			campaign.WorkingHours.Add(DayOfWeek.Wednesday, new TimePeriod());
			campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod());
			campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod());
			
			_outboundCampaignRepository.Stub(x => x.Get(_campaignViewModel.Id.Value)).Return(campaign);
			_campaignViewModel.WorkingHours = new List<CampaignWorkingHour>()
			{
				new CampaignWorkingHour(){WeekDay = DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)},
				new CampaignWorkingHour(){WeekDay = DayOfWeek.Tuesday, StartTime =new TimeSpan(9,0,0), EndTime = new TimeSpan(17,0,0)}
			};

			var target = new OutboundCampaignMapper(_outboundCampaignRepository, _userTimeZone);
			var result = target.Map(_campaignViewModel);

			result.WorkingHours.Count.Should().Be.EqualTo(2);
			result.WorkingHours.ToList()[0].Key.Should().Be.EqualTo(DayOfWeek.Monday);
			result.WorkingHours.ToList()[1].Key.Should().Be.EqualTo(DayOfWeek.Tuesday);
		}
	}
}
