using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


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
      private IOutboundCampaign _campaign;

		[SetUp]
		public void Setup()
		{
			var skill = SkillFactory.CreateSkill("mySkill", SkillTypeFactory.CreateSkillType(), 15, TimeZoneInfo.Utc, TimeSpan.Zero);
			_skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			_scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			_createOrUpdateSkillDays = MockRepository.GenerateMock<ICreateOrUpdateSkillDays>();
			_target = new OutboundPeriodMover(_skillDayRepository, _scenarioRepository, _createOrUpdateSkillDays);
			_defultScenario = ScenarioFactory.CreateScenario("scenario", false, false);

			_campaign = new Campaign
			{
				Name = "test",
				CallListLen = 100,
				TargetRate = 20,
				RightPartyConnectRate = 20,
				ConnectRate = 20,
				RightPartyAverageHandlingTime = 120,
				UnproductiveTime = 30,
				ConnectAverageHandlingTime = (int)TimeSpan.FromMinutes(100).TotalSeconds,
				Skill = skill
			};

			_campaign.WorkingHours.Add(DayOfWeek.Friday, new TimePeriod(10, 0, 15, 0));
			_campaign.SpanningPeriod = new DateTimePeriod(2015, 6, 15, 2015, 6, 15);
		}

		[Test]
		public void ShouldDeleteSkilldaysForOldPeriod()
		{
			_scenarioRepository.Stub(x => x.LoadDefaultScenario()).Return(_defultScenario);
			_createOrUpdateSkillDays.Stub(
				x => x.Create(_campaign.Skill, _campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), _campaign.CampaignTasks(), _campaign.AverageTaskHandlingTime(), _campaign.WorkingHours));

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
				x => x.Create(_campaign.Skill, _campaign.SpanningPeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), _campaign.CampaignTasks(), _campaign.AverageTaskHandlingTime(), _campaign.WorkingHours));

			_target.Move(_campaign, new DateOnlyPeriod(2015, 6, 8, 2015, 6, 8));

			Assert.That(!_campaign.GetManualProductionPlan(new DateOnly(2015, 6, 8)).HasValue);
			Assert.That(!_campaign.GetActualBacklog(new DateOnly(2015, 6, 8)).HasValue);
		}
	}
}