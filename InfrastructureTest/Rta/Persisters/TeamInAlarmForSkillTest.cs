using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[DatabaseTest]
	[TestFixture]
	public class TeamInAlarmForSkillTest
	{
		public MutableNow Now;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public IGroupingReadOnlyRepository Groupings;
		public IAgentStateReadModelPersister StatePersister;
		public ITeamInAlarmReader Target;

		[Test]
		public void ShouldLoad()
		{
			Now.Is("2016-10-17 08:10");
			var siteId = Guid.NewGuid();
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = siteId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(siteId, new[] { currentSkillId }))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			var result = WithUnitOfWork.Get(() => Target.ReadForSkills(siteId, new[] { currentSkillId })).Single();
			
			result.TeamId.Should().Be(teamId);
			result.Count.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyLoadForWantedSkill()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Email")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var siteId = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					TeamId = teamId1, 
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					TeamId = teamId2,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(siteId, new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}

		[Test]
		public void ShouldOnlyCountAgentsInAlarm()
		{
			Now.Is("2016-10-17 08:10");
			Database
				.WithAgent("Ashley")
				.WithSkill("Phone")
				.WithAgent("Pierre")
				.WithSkill("Phone");
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var ashleyId = Database.PersonIdFor("Ashley");
			var pierreId = Database.PersonIdFor("Pierre");
			var phoneSkillId = Database.SkillIdFor("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = false
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadForSkills(siteId, new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}

		[Test]
		public void ShouldNotCountDeletedAgents()
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
				Groupings.UpdateGroupingReadModel(new[] { ashleyId, pierreId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = pierreId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = ashleyId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc()
				});
			});
			WithUnitOfWork.Do(() => StatePersister.SetDeleted(ashleyId, "2016-10-17 08:30".Utc()));

			WithUnitOfWork.Get(() => Target.ReadForSkills(siteId, new[] { phoneSkillId }))
				.Single().Count.Should().Be(1);
		}
	}
}