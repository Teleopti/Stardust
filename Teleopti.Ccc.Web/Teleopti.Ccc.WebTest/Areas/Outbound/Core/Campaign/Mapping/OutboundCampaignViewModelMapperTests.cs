using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignViewModelMapperTests
	{
		private IList<Domain.Outbound.Campaign> _campaigns;
		private OutboundCampaignViewModelMapper _target;
		private Domain.Outbound.Campaign _campaign;
		private ISkill _createdSkill;

		[SetUp]
		public void Setup()
		{
			_createdSkill = SkillFactory.CreateSkillWithId("skill1");
			_createdSkill.TimeZone = TimeZoneInfo.Utc;

			_campaign = new Domain.Outbound.Campaign {Name = "myCampaign", Skill = _createdSkill};
			_campaign.WorkingHours.Add(DayOfWeek.Monday, new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(17,0,0)));
			_campaign.WorkingHours.Add(DayOfWeek.Tuesday, new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(17,0,0)));
			_campaign.WorkingHours.Add(DayOfWeek.Wednesday, new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(17,0,0)));
			_campaign.WorkingHours.Add(DayOfWeek.Thursday, new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(17,0,0)));
			_campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(new TimeSpan(9,0,0), new TimeSpan(17,0,0)));

			_campaigns = new List<Domain.Outbound.Campaign> {_campaign};
			_target = new OutboundCampaignViewModelMapper();
		}

		[Test]
		public void ShouldMapId()
		{
			var result = _target.Map(_campaigns);
			result.First().Id.Should().Be.EqualTo(_campaign.Id);
		}

		[Test]
		public void ShouldMapName()
		{
			var result = _target.Map(_campaigns);
			result.First().Name.Should().Be.EqualTo(_campaign.Name);
		}

		[Test]
		public void ShouldMapActivity()
		{
			var result = _target.Map(_campaigns);
			var target = result.First().Activity;

			target.Id.Should().Be.EqualTo(_createdSkill.Activity.Id);
		}

		[Test]
		public void ShouldMapCallListLen()
		{
			var result = _target.Map(_campaigns);
			result.First().CallListLen.Should().Be.EqualTo(_campaign.CallListLen);
		}

		[Test]
		public void ShouldMapTargetRate()
		{
			var result = _target.Map(_campaigns);
			result.First().TargetRate.Should().Be.EqualTo(_campaign.TargetRate);
		}

		[Test]
		public void ShouldMapConnectRate()
		{
			var result = _target.Map(_campaigns);
			result.First().ConnectRate.Should().Be.EqualTo(_campaign.ConnectRate);
		}

		[Test]
		public void ShouldMapRightPartyConnectRate()
		{
			var result = _target.Map(_campaigns);
			result.First().RightPartyConnectRate.Should().Be.EqualTo(_campaign.RightPartyConnectRate);
		}

		[Test]
		public void ShouldMapConnectAverageHandlingTime()
		{
			var result = _target.Map(_campaigns);
			result.First().ConnectAverageHandlingTime.Should().Be.EqualTo(_campaign.ConnectAverageHandlingTime);
		}

		[Test]
		public void ShouldMapRightPartyAverageHandlingTime()
		{
			var result = _target.Map(_campaigns);
			result.First().RightPartyAverageHandlingTime.Should().Be.EqualTo(_campaign.RightPartyAverageHandlingTime);
		}

		[Test]
		public void ShouldMapUnproductiveTime()
		{
			var result = _target.Map(_campaigns);
			result.First().UnproductiveTime.Should().Be.EqualTo(_campaign.UnproductiveTime);
		}

		[Test]
		public void ShouldMapStartDate()
		{
			var result = _target.Map(_campaigns);
			result.First().StartDate.Should().Be.EqualTo(new DateOnly(_campaign.SpanningPeriod.StartDateTime.Date));
		}

		[Test]
		public void ShouldMapEndDate()
		{
			var result = _target.Map(_campaigns);
			result.First().EndDate.Should().Be.EqualTo(new DateOnly(_campaign.SpanningPeriod.EndDateTime.Date));
		}

		[Test]
		public void ShouldMapDayOfWeek()
		{
			var result = _target.Map(_campaigns);
			var workingHours = result.First().WorkingHours.ToList();

			for (var i = 0; i < 5; ++i)
			{
				_campaigns[0].WorkingHours.ContainsKey(workingHours[i].WeekDay).Should().Be.True();
			}
		}		
		
		[Test]
		public void ShouldMapTimePeriod()
		{
			var result = _target.Map(_campaigns);
			var workingHours = result.First().WorkingHours.ToList();

			for (var i = 0; i < 5; ++i)
			{
				_campaigns[0].WorkingHours[workingHours[i].WeekDay].Should().Be.EqualTo(new TimePeriod(workingHours[i].StartTime, workingHours[i].EndTime));
			}
		}
	}
}
