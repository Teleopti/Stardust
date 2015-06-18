using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignViewModelMapperTests
	{
		private IEnumerable<Domain.Outbound.Campaign> _campaigns;
		private OutboundCampaignViewModelMapper _target;
		private Domain.Outbound.Campaign _campaign;
		private IList<ISkill> _skills;
		private ISkill _selectedSkill;

		[SetUp]
		public void Setup()
		{
			var skill1 = SkillFactory.CreateSkillWithId("skill1");
			var skill2 = SkillFactory.CreateSkillWithId("skill2");
			var skill3 = SkillFactory.CreateSkillWithId("skill3");

			_skills = new List<ISkill> {skill1, skill2, skill3};
			_selectedSkill = skill1;

			_campaign = new Domain.Outbound.Campaign("myCampaign", _selectedSkill);
			var workingPeriod = new CampaignWorkingPeriod() {TimePeriod = new TimePeriod()};
			workingPeriod.AddAssignment(new CampaignWorkingPeriodAssignment(){WeekdayIndex = DayOfWeek.Monday});
			_campaign.AddWorkingPeriod(workingPeriod);
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
		public void ShouldMapSkillId()
		{
			var result = _target.Map(_campaigns);
			var targetSkill = result.First().Skills.First();

			targetSkill.Id.Should().Be.EqualTo(_skills.First().Id);
		}

		[Test]
		public void ShouldMapSkillName()
		{
			var result = _target.Map(_campaigns);
			var targetSkill = result.First().Skills.First();

			targetSkill.SkillName.Should().Be.EqualTo(_skills.First().Name);
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
			result.First().StartDate.Should().Be.EqualTo(_campaign.SpanningPeriod.StartDate);
		}		
		
		[Test]
		public void ShouldMapEndDate()
		{
			var result = _target.Map(_campaigns);
			result.First().EndDate.Should().Be.EqualTo(_campaign.SpanningPeriod.EndDate);
		}

		[Test]
		public void ShouldMapCampaignWorkingPeriodId()
		{
			var result = _target.Map(_campaigns);
			result.First().CampaignWorkingPeriods.First().Id.Should().Be.EqualTo(_campaign.CampaignWorkingPeriods.First().Id);
		}

		[Test]
		public void ShouldMapCampaignWorkingPeriodStartTime()
		{
			var expactedTime = new TimeSpan(6, 0, 0);
			_campaigns.First().CampaignWorkingPeriods.First().TimePeriod = new TimePeriod(expactedTime, new TimeSpan(18, 0, 0));
			var result = _target.Map(_campaigns);

			result.First().CampaignWorkingPeriods.First().StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expactedTime, CultureInfo.CurrentCulture));
		}		
		
		[Test]
		public void ShouldMapCampaignWorkingPeriodEndTime()
		{
			var expactedTime = new TimeSpan(18, 0, 0);
			_campaigns.First().CampaignWorkingPeriods.First().TimePeriod = new TimePeriod(new TimeSpan(6, 0, 0), expactedTime);
			var result = _target.Map(_campaigns);

			result.First().CampaignWorkingPeriods.First().EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(expactedTime, CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapCampaignWorkingPeriodAssignmentsCount()
		{
			var result = _target.Map(_campaigns);
			var assignments = result.First().CampaignWorkingPeriods.First().WorkingPeroidAssignments;
			assignments.Count().Should().Be.EqualTo(_campaign.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.Count);
		}			
		
		[Test]
		public void ShouldMapCampaignWorkingPeriodAssignmentId()
		{
			var result = _target.Map(_campaigns);
			var assignment = result.First().CampaignWorkingPeriods.First().WorkingPeroidAssignments.First();
			assignment.Id.Should().Be.EqualTo(_campaign.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.First().Id);
		}		
		
		[Test]
		public void ShouldMapCampaignWorkingPeriodAssignmentWeekday()
		{
			var result = _target.Map(_campaigns);
			var assignment = result.First().CampaignWorkingPeriods.First().WorkingPeroidAssignments.First();
			assignment.WeekDay.Should().Be.EqualTo(_campaign.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.First().WeekdayIndex);
		}
	}
}
