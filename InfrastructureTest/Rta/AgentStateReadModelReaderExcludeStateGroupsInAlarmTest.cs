using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStateReadModelReaderExcludeStateGroupsInAlarmTest
	{
		public IAgentStateReadModelPersister Persister;
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public WithUnitOfWork WithUnitOfWork;
		public MutableNow Now;
		public IAgentStateReadModelReader Target;


		[Test]
		public void ShouldLoadForTeams()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = Guid.NewGuid()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = null,
				IsRuleAlarm = false,
				StateGroupId = Guid.NewGuid()
			});
			
			Target.LoadAlarmsForTeams(new[] {teamId}, new Guid?[] {loggedOut})
				.Single().PersonId.Should().Be(personId1);
		}


		[Test]
		public void ShouldLoadForSites()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = Guid.NewGuid()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = null,
				IsRuleAlarm = false,
				StateGroupId = Guid.NewGuid()
			});

			Target.LoadAlarmsForSites(new[] {siteId}, new Guid?[] {loggedOut})
				.Single().PersonId.Should().Be(personId1);
		}
		
		[Test]
		public void ShouldLoadForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var skill = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId1, personId2 });
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadAlarmsForSkills(new[] { skill }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(personId2);
		}

		[Test]
		public void ShouldLoadStatesWithoutStateGroupForTeams()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});

			Target.LoadAlarmsForTeams(new[] { teamId }, new Guid?[] { loggedOut })
				.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStateWithoutStateGroupForSites()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});

			Target.LoadAlarmsForSites(new[] { siteId }, new Guid?[] { loggedOut })
				.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStateWithoutStateGroupForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var skill = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId1, personId2 });
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = null
				});
			});

			WithUnitOfWork.Get(() => Target.LoadAlarmsForSkills(new[] { skill }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(personId2);
		}
		
		[Test]
		public void ShouldoExcludeStateWithoutStateGroupForTeam()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});

			Target.LoadAlarmsForTeams(new[] { teamId }, new Guid?[] { null })
				.Single().PersonId.Should().Be(personId1);
		}


		[Test]
		public void ShouldExcludeBothWithAndWithoutStateGroupForTeam()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			var training = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = training
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});

			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				TeamId = teamId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});


			Target.LoadAlarmsForTeams(new[] { teamId }, new Guid?[] { null, loggedOut })
				.Single().PersonId.Should().Be(personId1);
		}


		[Test]
		public void ShouldExcludeStateWithoutStateGroupForSite()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});

			Target.LoadAlarmsForSites(new[] { siteId }, new Guid?[] { null })
				.Single().PersonId.Should().Be(personId1);
		}


		[Test]
		public void ShouldExcludeBothWithAndWithoutStateGroupForSite()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			var training = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = training
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = null
			});

			Persister.Persist(new AgentStateReadModelForTest
			{
				PersonId = Guid.NewGuid(),
				SiteId = siteId,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = loggedOut
			});


			Target.LoadAlarmsForSites(new[] { siteId }, new Guid?[] { null, loggedOut })
				.Single().PersonId.Should().Be(personId1);
		}
		
		[Test]
		public void ShouldExcludeStateWithoutStateGroupForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var skill = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId1, personId2 });
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = null
				});
			});

			WithUnitOfWork.Get(() => Target.LoadAlarmsForSkills(new[] { skill }, new Guid?[] { null }))
				.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldExcludeBothWithAndWithoutStateGroupForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone")
				.WithAgent("agent3")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var personId3 = Database.PersonIdFor("agent3");
			var skill = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			var training = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId1, personId2, personId3 });
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					AlarmStartTime = "2016-06-15 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = training
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = null
				});
				Persister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId3,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.LoadAlarmsForSkills(new[] { skill }, new Guid?[] { null, loggedOut }))
				.Single().PersonId.Should().Be(personId1);
		}

	}
}