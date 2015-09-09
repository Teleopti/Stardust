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
    class OutboundCampaignRepositoryTest : RepositoryTest<IOutboundCampaign>
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

        protected override IOutboundCampaign CreateAggregateWithCorrectBusinessUnit()
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
				SpanningPeriod = new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc))
			};

			return campaign;
		}

        protected override void VerifyAggregateGraphProperties(IOutboundCampaign loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo("Sept Sales");
		}

        protected override Repository<IOutboundCampaign> TestRepository(IUnitOfWork unitOfWork)
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
			var expectedPeriod = new DateTimePeriod(2015, 6, 18, 2015, 7, 18);
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
		public void ShouldGetPlannedCampaignsWithSequence()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(1).Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(2).Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetPlannedCampaigns();

			result.Count.Should().Be.EqualTo(2);
			result[0].Id.Should().Be.EqualTo(campaign1.Id);
			result[1].Id.Should().Be.EqualTo(campaign2.Id);
		}		
		
		[Test]
		public void ShouldGetDoneCampaignsWithSequence()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(-8).Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-9).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(-8).Ticks, DateTimeKind.Utc)));
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetDoneCampaigns();

			result.Count.Should().Be.EqualTo(2);
			result[0].Id.Should().Be.EqualTo(campaign2.Id);
			result[1].Id.Should().Be.EqualTo(campaign1.Id);
		}		
		
		[Test]
		public void ShouldGetOnGoingCampaignsWithSequence()
		{
			var campaign1 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(-8).Ticks, DateTimeKind.Utc)));
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(8).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var result = repository.GetOnGoingCampaigns();

			result.Count.Should().Be.EqualTo(2);
			result[0].Id.Should().Be.EqualTo(campaign2.Id);
			result[1].Id.Should().Be.EqualTo(campaign1.Id);
		}

		[Test]
		public void ShouldNotGetDeletedPlannedCampaign()
		{
			var deletedCamapign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(1).Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(2).Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			deleteCampaign(repository, deletedCamapign);

			var result = repository.GetPlannedCampaigns();

			result[0].Id.Should().Be.EqualTo(campaign2.Id);
		}

		[Test]
		public void ShouldNotGetDeletedOnGoingCampaign()
		{
			var deletedCamapign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			deleteCampaign(repository, deletedCamapign);

			var result = repository.GetOnGoingCampaigns();

			result[0].Id.Should().Be.EqualTo(campaign2.Id);
		}

		[Test]
		public void ShouldNotGetDeletedDoneCampaign()
		{
			var deletedCamapign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(-8).Ticks, DateTimeKind.Utc)));
			var campaign2 = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-9).Ticks, DateTimeKind.Utc), new DateTime(DateTime.Today.AddDays(-8).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			deleteCampaign(repository, deletedCamapign);

			var result = repository.GetDoneCampaigns();

			result[0].Id.Should().Be.EqualTo(campaign2.Id);
		}

		[Test]
		public void ShouldSaveActualBacklog()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SetActualBacklog(DateOnly.Today, TimeSpan.FromHours(10));
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.GetActualBacklog(DateOnly.Today).Should().Be.EqualTo(TimeSpan.FromHours(10));
		}

		[Test]
		public void ShouldRemoveActualBacklog()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SetActualBacklog(DateOnly.Today, TimeSpan.FromHours(10));
			campaign.ClearActualBacklog(DateOnly.Today);
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.GetActualBacklog(DateOnly.Today).Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldGetCampaignStartEarlierThanPeriod()
		{
			var campaign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc), 
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-5).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(20).Ticks, DateTimeKind.Utc)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}

		[Test]
		public void ShouldGetCampaignEndLaterThanPeriod()
		{
			var campaign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc),
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-15).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(5).Ticks, DateTimeKind.Utc)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldGetCampaignWithinPeriod()
		{
			var campaign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc),
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-20).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(20).Ticks, DateTimeKind.Utc)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}			
		
		[Test]
		public void ShouldGetCampaignOverCrossPeriod()
		{
			var campaign = createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc),
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-5).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(5).Ticks, DateTimeKind.Utc)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldNotGetCampaignCampleteEarlierThanPeriod()
		{
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc),
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(20).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(30).Ticks, DateTimeKind.Utc)));

			result.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldNotGetCampaignStartLaterThanPeriod()
		{
			createCampaignWithSpanningPeriod(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-10).Ticks, DateTimeKind.Utc),
																									 new DateTime(DateTime.Today.AddDays(10).Ticks, DateTimeKind.Utc)));
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var result = repository.GetCampaigns(new DateTimePeriod(new DateTime(DateTime.Today.AddDays(-20).Ticks, DateTimeKind.Utc),
																					  new DateTime(DateTime.Today.AddDays(-15).Ticks, DateTimeKind.Utc)));

			result.Count.Should().Be.EqualTo(0);
		}

		private void deleteCampaign(IOutboundCampaignRepository repository, IOutboundCampaign campaign)
		{
			repository.Remove(campaign);
			PersistAndRemoveFromUnitOfWork(campaign);
		}

      private IOutboundCampaign createCampaignWithSpanningPeriod(DateTimePeriod period)
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SpanningPeriod = period;

			PersistAndRemoveFromUnitOfWork(campaign);
			return campaign;
		}

      private IOutboundCampaign CreateCampaignWithWorkingHours(DayOfWeek weekday, TimePeriod period)
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
