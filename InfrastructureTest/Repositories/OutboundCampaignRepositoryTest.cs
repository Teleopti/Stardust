using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTestsEx;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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

		private IPerson _user;
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
				CampaignStatus = Domain.Outbound.CampaignStatus.Draft,
				CampaignWorkingPeriods = new List<CampaignWorkingPeriod>(),				
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
	}
}
