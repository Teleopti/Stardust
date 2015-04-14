using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
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
				StartDate = DateOnly.Today,
				EndDate = DateOnly.Today,
				CampaignStatus = CampaignStatus.Draft,				
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


		protected Campaign CreateCampaignWithWorkingPeriodAndAssignment()
		{
			var campaign = CreateAggregateWithCorrectBusinessUnit();

			campaign.AddWorkingPeriod(new CampaignWorkingPeriod
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)),
			});

			foreach (var workingPeriod in campaign.CampaignWorkingPeriods)
			{

				var assignment = new CampaignWorkingPeriodAssignment
				{
					WeekdayIndex = DayOfWeek.Monday
				};

				workingPeriod.AddAssignment(assignment);
			}
			PersistAndRemoveFromUnitOfWork(campaign);
			return campaign;
		}

		[Test]
		public  void ShouldReturnCampaignWithWorkingPeriodAssignments()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();
			var repository = new OutboundCampaignRepository(UnitOfWork);

			var loadedCampagin = repository.Get(campaign.Id.GetValueOrDefault());

			loadedCampagin.CampaignWorkingPeriods.Count().Should().Be.EqualTo(1);
			loadedCampagin.CampaignWorkingPeriods.First().TimePeriod.Should().Be.EqualTo(
				new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17))
				);

			loadedCampagin.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.Count().Should()
				.Be.EqualTo(1);

			loadedCampagin.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.First().WeekdayIndex
				.Should().Be.EqualTo(DayOfWeek.Monday);

		}

		[Test]
		public void CanAddWorkingPeriodToCampaign()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampagin = repository.Get(campaign.Id.GetValueOrDefault());

			var workingPeriod = new CampaignWorkingPeriod
			{
				TimePeriod = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17))
			};
			loadedCampagin.AddWorkingPeriod(workingPeriod);
			PersistAndRemoveFromUnitOfWork(loadedCampagin);

			var reloadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());
			reloadedCampaign.CampaignWorkingPeriods.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void CanRemoveWorkingPeriodFromCampaign()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampagin = repository.Get(campaign.Id.GetValueOrDefault());

			var workingPeriod = loadedCampagin.CampaignWorkingPeriods.First();
			loadedCampagin.RemoveWorkingPeriod(workingPeriod);
			PersistAndRemoveFromUnitOfWork(loadedCampagin);

			var reloadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());
			reloadedCampaign.CampaignWorkingPeriods.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void CanRemoveWorkingPeriodAssignmentFromCampaign()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();

			var repository = new OutboundCampaignRepository(UnitOfWork);

			var loadedCampagin = repository.Get(campaign.Id.GetValueOrDefault());

			var assignment = loadedCampagin.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.First();
			loadedCampagin.CampaignWorkingPeriods.First().RemoveAssignment(assignment);
			PersistAndRemoveFromUnitOfWork(loadedCampagin);

			var reloadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			reloadedCampaign.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments.Count().Should()
				.Be.EqualTo(0);
		}

		[Test]
		public void CanAddWorkingPeriodAssignmentToCampaign()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();

			var repository = new OutboundCampaignRepository(UnitOfWork);

			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());
			var assignment = new CampaignWorkingPeriodAssignment {WeekdayIndex = DayOfWeek.Friday};
			loadedCampaign.CampaignWorkingPeriods.First().AddAssignment(assignment);

			PersistAndRemoveFromUnitOfWork(loadedCampaign);

			var reloadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());

			reloadedCampaign.Should().Be.EqualTo(loadedCampaign);

			reloadedCampaign.CampaignWorkingPeriods.First().CampaignWorkingPeriodAssignments
				.Count().Should().Be.EqualTo(2);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CannotAddDuplicateWorkingPeriodAssignmentToCampaign()
		{
			var campaign = CreateCampaignWithWorkingPeriodAndAssignment();
			var repository = new OutboundCampaignRepository(UnitOfWork);
			var loadedCampaign = repository.Get(campaign.Id.GetValueOrDefault());
			var assignment = new CampaignWorkingPeriodAssignment { WeekdayIndex = DayOfWeek.Monday };
			loadedCampaign.CampaignWorkingPeriods.First().AddAssignment(assignment);			
		}



	}
}
