using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	[DatabaseTest]
	[TestFixture]
	public class TeamCardReaderActiveTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public IGroupingReadOnlyRepository Groupings;
		public IAgentStateReadModelPersister StatePersister;
		public ITeamCardReader Target;

		[Test]
		public void ShouldNotCountAgentsWithoutAssociation()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Phone")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {ashleyId, pierreId});
				StatePersister.UpsertToActiveWithState(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.UpsertToActiveWithState(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});
			WithUnitOfWork.Do(() => StatePersister.UpsertNoAssociation(ashleyId));

			WithUnitOfWork.Get(() => Target.Read(siteId, new[] {phoneSkillId}))
				.Single().InAlarmCount.Should().Be(1);
		}

		[Test]
		public void ShouldNotCountAgentsWithoutName()
		{
			var siteId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
				StatePersister.UpsertAssociation(new AssociationInfo
				{
					PersonId = Guid.Empty,
					BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
					SiteId = siteId
				})
			);

			WithUnitOfWork.Get(() => Target.Read(siteId))
				.Should().Be.Empty();
		}
	}
}