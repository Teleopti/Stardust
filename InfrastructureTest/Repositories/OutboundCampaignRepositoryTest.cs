using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	class OutboundCampaignRepositoryTest : RepositoryTest<Campaign>
	{
		private SkillType _skillType;
		private ISkill _skill;
		private IActivity _activity;


		protected override void ConcreteSetup()
		{			
			_skillType = SkillTypeFactory.CreateSkillType();
			PersistAndRemoveFromUnitOfWork(_skillType);
			_activity = new Activity("The test") { DisplayColor = Color.Honeydew };
			PersistAndRemoveFromUnitOfWork(_activity);
			_skill = SkillFactory.CreateSkill("test!", _skillType, 15);
			_skill.Activity = _activity;
			PersistAndRemoveFromUnitOfWork(_skill);
		}

		protected override Campaign CreateAggregateWithCorrectBusinessUnit()
		{

			var campaign = new Campaign()
			{
				Name = "Sept Sales",
				CallListLen = 100,
				TargetRate = 50,
				Skill = _skill,
				ConnectRate = 20,
				RightPartyConnectRate = 20,
				ConnectAverageHandlingTime = 30,
				RightPartyAverageHandlingTime = 120,
				UnproductiveTime = 30,
				SpanningPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue)
			};

			return campaign;
		}

		protected override void VerifyAggregateGraphProperties(Campaign loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo("Sept Sales");
			
		}

		protected override Repository<Campaign> TestRepository(IUnitOfWork unitOfWork)
		{
			return new OutboundCampaignRepository(unitOfWork);
		}

		[Test]
		public void CanAddWorkingHoursToCampaign()
		{
			var expectedWeekday = DayOfWeek.Monday;
			var expectePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var campaign = CreateCampaignWithWorkingHours(expectedWeekday, expectePeriod);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());
		
			loadedCampaign.WorkingHours.ContainsKey(expectedWeekday).Should().Be.True();
			loadedCampaign.WorkingHours[expectedWeekday].Should().Be.EqualTo(expectePeriod);
		}

		[Test]
		public void ShouldSetSpanningPeriod()
		{
			var expectedPeriod = new DateOnlyPeriod(2015, 6, 18, 2015, 7, 18);
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SpanningPeriod = expectedPeriod;
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.SpanningPeriod.Should().Be.EqualTo(expectedPeriod);
		}

		private Campaign CreateCampaignWithWorkingHours(DayOfWeek weekday, TimePeriod period)
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();

			var workingHours = new Dictionary<DayOfWeek, TimePeriod>();
			workingHours.Add(weekday, period);
			campaign.WorkingHours = workingHours;

			PersistAndRemoveFromUnitOfWork(campaign);
			return campaign;
		}
	}
}
