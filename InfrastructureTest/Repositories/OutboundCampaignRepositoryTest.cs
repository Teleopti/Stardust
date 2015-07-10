using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
		public void ShouldPersistCampaignName()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.Name.Should().Be.EqualTo(campaign.Name);
		}			
		
		[Test]
		public void ShouldPersistCallListLength()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.CallListLen.Should().Be.EqualTo(campaign.CallListLen);
		}			
		
		[Test]
		public void ShouldPersistTargetRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.TargetRate.Should().Be.EqualTo(campaign.TargetRate);
		}		
		
		[Test]
		public void ShouldPersistSkill()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.Skill.Id.Should().Be.EqualTo(campaign.Skill.Id);
		}			
		
		[Test]
		public void ShouldPersistConnectRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.ConnectRate.Should().Be.EqualTo(campaign.ConnectRate);
		}			
		
		[Test]
		public void ShouldPersistRightPartyConnectRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.RightPartyConnectRate.Should().Be.EqualTo(campaign.RightPartyConnectRate);
		}			
		
		[Test]
		public void ShouldPersistConnectAverageHandlingTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.ConnectAverageHandlingTime.Should().Be.EqualTo(campaign.ConnectAverageHandlingTime);
		}			
		
		[Test]
		public void ShouldPersistRightPartyConectAverageHandlingTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.RightPartyAverageHandlingTime.Should().Be.EqualTo(campaign.RightPartyAverageHandlingTime);
		}				
		
		[Test]
		public void ShouldPersistUnproductiveTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.UnproductiveTime.Should().Be.EqualTo(campaign.UnproductiveTime);
		}	
		
		[Test]
		public void ShouldPersistWorkingHours()
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
		public void ShouldPersistSpanningPeriod()
		{
			var expectedPeriod = new DateOnlyPeriod(2015, 6, 18, 2015, 7, 18);
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SpanningPeriod = expectedPeriod;
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.SpanningPeriod.Should().Be.EqualTo(expectedPeriod);
		}

		[Test]
		public void ShouldGetCampaignTask()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			var expectedCampaignTask = campaign.CallListLen*campaign.TargetRate/campaign.RightPartyConnectRate;

			campaign.CampaignTasks().Should().Be.EqualTo(expectedCampaignTask);
		}		
		
		[Test]
		public void ShouldGetAverageTaskHandlingTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.CallListLen = 1000;
			campaign.TargetRate = 50;
			campaign.RightPartyAverageHandlingTime = 100;
			campaign.UnproductiveTime = 10;
			campaign.RightPartyConnectRate = 20;
			campaign.ConnectAverageHandlingTime = 10;
			campaign.ConnectRate = 100;

			campaign.AverageTaskHandlingTime().Should().Be.EqualTo(new TimeSpan(0, 0, 38));
		}

		[Test]
		public void ShouldGetPlannedCampaigns()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(1), DateOnly.MaxValue));
			createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetPlannedCampaigns();

			result.Count.Should().Be.EqualTo(1);
			result[0].Id.Should().Be.EqualTo(campaign1.Id);
		}		
		
		[Test]
		public void ShouldGetDoneCampaigns()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(-8)));
			createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetDoneCampaigns();

			result.Count.Should().Be.EqualTo(1);
			result[0].Id.Should().Be.EqualTo(campaign1.Id);
		}		
		
		[Test]
		public void ShouldGetOnGoingCampaigns()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today));
			var campaign2 = createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue));
			createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(-8)));
			createCampaignWithSpanningPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(8), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetOnGoingCampaigns();

			result.Count.Should().Be.EqualTo(2);
			foreach (var shouldTrue in result.Select(campaign => campaign.Id == campaign1.Id || campaign.Id == campaign2.Id))
			{
				shouldTrue.Should().Be.True();
			}
		}

		private Campaign createCampaignWithSpanningPeriod(DateOnlyPeriod period)
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SpanningPeriod = period;

			PersistAndRemoveFromUnitOfWork(campaign);
			return campaign;
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
