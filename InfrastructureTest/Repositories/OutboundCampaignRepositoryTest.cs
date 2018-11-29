using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

using Campaign = Teleopti.Ccc.Domain.Outbound.Campaign;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
    public class OutboundCampaignRepositoryTest : RepositoryTest<IOutboundCampaign>
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
			var campaign = new Campaign
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
				SpanningPeriod = new DateTimePeriod(new DateTime(DateTime.Today.Ticks, DateTimeKind.Utc), new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc)),
				BelongsToPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue)
			};

			return campaign;
		}

        protected override void VerifyAggregateGraphProperties(IOutboundCampaign loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo("Sept Sales");
		}

        protected override Repository<IOutboundCampaign> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new OutboundCampaignRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldPersistCampaignName()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.Name.Should().Be.EqualTo(campaign.Name);
		}			
		
		[Test]
		public void ShouldPersistCallListLength()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.CallListLen.Should().Be.EqualTo(campaign.CallListLen);
		}			
		
		[Test]
		public void ShouldPersistTargetRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.TargetRate.Should().Be.EqualTo(campaign.TargetRate);
		}		
		
		[Test]
		public void ShouldPersistSkill()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.Skill.Id.Should().Be.EqualTo(campaign.Skill.Id);
		}			
		
		[Test]
		public void ShouldPersistConnectRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.ConnectRate.Should().Be.EqualTo(campaign.ConnectRate);
		}			
		
		[Test]
		public void ShouldPersistRightPartyConnectRate()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.RightPartyConnectRate.Should().Be.EqualTo(campaign.RightPartyConnectRate);
		}			
		
		[Test]
		public void ShouldPersistConnectAverageHandlingTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.ConnectAverageHandlingTime.Should().Be.EqualTo(campaign.ConnectAverageHandlingTime);
		}			
		
		[Test]
		public void ShouldPersistRightPartyConectAverageHandlingTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.RightPartyAverageHandlingTime.Should().Be.EqualTo(campaign.RightPartyAverageHandlingTime);
		}				
		
		[Test]
		public void ShouldPersistUnproductiveTime()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.UnproductiveTime.Should().Be.EqualTo(campaign.UnproductiveTime);
		}	
		
		[Test]
		public void ShouldPersistWorkingHours()
		{
			var expectedWeekday = DayOfWeek.Monday;
			var expectePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(15, 0, 0));
			var campaign = CreateCampaignWithWorkingHours(expectedWeekday, expectePeriod);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
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

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.SpanningPeriod.Should().Be.EqualTo(expectedPeriod);
		}		
		
		[Test]
		public void ShouldPersistBelongsToPeriod()
		{
			var expectedPeriod = new DateOnlyPeriod(2015, 6, 18, 2015, 7, 18);
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.BelongsToPeriod = expectedPeriod;
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.BelongsToPeriod.Should().Be.EqualTo(expectedPeriod);
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
		public void ShouldSaveActualBacklog()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.SetActualBacklog(DateOnly.Today, TimeSpan.FromHours(10));
			PersistAndRemoveFromUnitOfWork(campaign);

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
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

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampaign.GetActualBacklog(DateOnly.Today).Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldGetCampaignSortByName()
		{
			createCampaignWithName("b");
			createCampaignWithName("a");

			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue));

			result[0].Name.Should().Be.EqualTo("a");
			result[1].Name.Should().Be.EqualTo("b");
		}

		[Test]
		public void ShouldNotGetDeletedCampaign()
		{
			var deletedCamapign = createCampaignWithName("deleted");
			var campaign2 = createCampaignWithName("campaign2");
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			deleteCampaign(repository, deletedCamapign);

			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today, DateOnly.MaxValue));

			result[0].Id.Should().Be.EqualTo(campaign2.Id);
		}

		[Test]
		public void ShouldGetCampaignStartEarlierThanPeriod()
		{
			var campaign = createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(-5), DateOnly.Today.AddDays(20)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}

		[Test]
		public void ShouldGetCampaignEndLaterThanPeriod()
		{
			var campaign = createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(-15), DateOnly.Today.AddDays(5)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldGetCampaignWithinPeriod()
		{
			var campaign = createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(-20), DateOnly.Today.AddDays(20)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}			
		
		[Test]
		public void ShouldGetCampaignOverCrossPeriod()
		{
			var campaign = createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(-5), DateOnly.Today.AddDays(5)));

			result[0].Id.Should().Be.EqualTo(campaign.Id);
		}		
		
		[Test]
		public void ShouldNotGetCampaignCampleteEarlierThanPeriod()
		{
			createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(20), DateOnly.Today.AddDays(30)));

			result.Count.Should().Be.EqualTo(0);
		}		
		
		[Test]
		public void ShouldNotGetCampaignStartLaterThanPeriod()
		{
			createCampaignWithBelongsToPeriod(new DateOnlyPeriod(DateOnly.Today.AddDays(-10), DateOnly.Today.AddDays(10)));
			var repository = new OutboundCampaignRepository(CurrUnitOfWork);
			var result = repository.GetCampaigns(new DateOnlyPeriod(DateOnly.Today.AddDays(-20), DateOnly.Today.AddDays(-15)));

			result.Count.Should().Be.EqualTo(0);
		}

		private void deleteCampaign(IOutboundCampaignRepository repository, IOutboundCampaign campaign)
		{
			repository.Remove(campaign);
			PersistAndRemoveFromUnitOfWork(campaign);
		}

      private IOutboundCampaign createCampaignWithBelongsToPeriod(DateOnlyPeriod period)
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.BelongsToPeriod = period;

			PersistAndRemoveFromUnitOfWork(campaign);
			return campaign;
		}
      
		private IOutboundCampaign createCampaignWithName(string name)
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();
			campaign.Name = name;

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
