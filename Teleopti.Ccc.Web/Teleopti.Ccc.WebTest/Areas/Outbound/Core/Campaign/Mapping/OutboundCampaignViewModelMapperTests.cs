using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.Core.Campaign.Mapping
{
	[TestFixture]
	internal class OutboundCampaignViewModelMapperTests
	{
		private ISkillRepository _skillRepository;
		private IEnumerable<Domain.Outbound.Campaign> _campaigns;
		private OutboundCampaignViewModelMapper _target;
		private Domain.Outbound.Campaign _campaign;
		private IList<ISkill> _skills;
		private ISkill _selectedSkill;
		private ICreateHourText _createHourText;

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
			_skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			_createHourText = MockRepository.GenerateMock<ICreateHourText>();
			_campaigns = new List<Domain.Outbound.Campaign> {_campaign};
			_skillRepository.Stub(x => x.LoadAll()).Return(_skills);
			_target = new OutboundCampaignViewModelMapper(_skillRepository, _createHourText);
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
		public void ShouldMapSkillCount()
		{
			var result = _target.Map(_campaigns);
			var targetSkills = result.First().Skills;

			targetSkills.Count().Should().Be.EqualTo(_skills.Count);
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
		public void ShouldMapSelectedSkill()
		{
			var result = _target.Map(_campaigns);
			var targetSkills = result.First().Skills;

			targetSkills.ForEach(x => x.IsSelected.Should().Be.EqualTo(x.Id == _selectedSkill.Id));
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
			result.First().StartDate.Should().Be.EqualTo(_campaign.StartDate);
		}

		[Test]
		public void ShouldMapEndDate()
		{
			var result = _target.Map(_campaigns);
			result.First().EndDate.Should().Be.EqualTo(_campaign.EndDate);
		}

		[Test]
		public void ShouldMapCampaignStatus()
		{
			var result = _target.Map(_campaigns);
			result.First().CampaignStatus.Should().Be.EqualTo(_campaign.CampaignStatus);
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
			var result = _target.Map(_campaigns);
			var expactedTime = "6:00am";
			_createHourText.Stub(x => x.CreateText(new DateTime())).IgnoreArguments().Return(expactedTime);

			result.First().CampaignWorkingPeriods.First().StartTime.Should().Be.EqualTo(expactedTime);
		}		
		
		[Test]
		public void ShouldMapCampaignWorkingPeriodEndTime()
		{
			var result = _target.Map(_campaigns);
			var expactedTime = "6:00pm";
			_createHourText.Stub(x => x.CreateText(new DateTime())).IgnoreArguments().Return(expactedTime);

			result.First().CampaignWorkingPeriods.First().EndTime.Should().Be.EqualTo(expactedTime);
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
