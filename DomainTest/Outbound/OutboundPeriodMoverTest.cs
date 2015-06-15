﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Outbound
{
	[TestFixture]
	public class OutboundPeriodMoverTest
	{
		private IOutboundPeriodMover _target;
		private ISkillDayRepository _skillDayRepository;
		private IScenarioRepository _scenarioRepository;
		private ICreateOrUpdateSkillDays _createOrUpdateSkillDays;
		private IScenario _defultScenario;
		private Campaign _campaign;

		[SetUp]
		public void Setup()
		{
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			_target = new OutboundPeriodMover(_skillDayRepository, _scenarioRepository, _createOrUpdateSkillDays);
			_defultScenario = ScenarioFactory.CreateScenario("scenario", false, false);

			_campaign = new Campaign
			{
				Name = "test",
				CallListLen = 100,
				ConnectAverageHandlingTime = (int)TimeSpan.FromMinutes(100).TotalSeconds
			};
			var campaignWorkingPeriod = new CampaignWorkingPeriod { TimePeriod = new TimePeriod(10, 0, 15, 0) };

			var campaignWorkingPeriodAssignmentFriday = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Monday };
			campaignWorkingPeriod.AddAssignment(campaignWorkingPeriodAssignmentFriday);

			_campaign.AddWorkingPeriod(campaignWorkingPeriod);
			_campaign.SetSpanningPeriod(new DateOnlyPeriod(2015, 6, 15, 2015, 6, 15));
		}

		[Test]
		public void ShouldDeleteSkilldaysForOldPeriod()
		{
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_defultScenario);
			_createOrUpdateSkillDays.Stub(
				x => x.Create(_campaign.Skill, _campaign.SpanningPeriod, _campaign.CampaignTasks(), _campaign.AverageTaskHandlingTime(), _campaign.CampaignWorkingPeriods));

			_target.Move(_campaign, new DateOnlyPeriod(2015, 6, 8, 2015, 6, 8));

			_skillDayRepository.AssertWasCalled(x => x.Delete(new DateOnlyPeriod(2015, 6, 8, 2015, 6, 8), _campaign.Skill, _defultScenario));
		}

		[Test]
		public void ShouldDeleteManualProductionPlanAndActualBacklogForOldPeriod()
		{
			_campaign.SetManualProductionPlan(new DateOnly(2015, 6, 8), TimeSpan.FromHours(1));
			Assert.That(_campaign.GetManualProductionPlan(new DateOnly(2015, 6, 8)).Equals(TimeSpan.FromHours(1)));

			_campaign.SetActualBacklog(new DateOnly(2015, 6, 8), TimeSpan.FromHours(1));
			Assert.That(_campaign.GetActualBacklog(new DateOnly(2015, 6, 8)).Equals(TimeSpan.FromHours(1)));

			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_defultScenario);
			_createOrUpdateSkillDays.Stub(
				x => x.Create(_campaign.Skill, _campaign.SpanningPeriod, _campaign.CampaignTasks(), _campaign.AverageTaskHandlingTime(), _campaign.CampaignWorkingPeriods));

			_target.Move(_campaign, new DateOnlyPeriod(2015, 6, 8, 2015, 6, 8));

			Assert.That(!_campaign.GetManualProductionPlan(new DateOnly(2015, 6, 8)).HasValue);
			Assert.That(!_campaign.GetActualBacklog(new DateOnly(2015, 6, 8)).HasValue);
		}
	}
}