using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
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
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
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

	}
}