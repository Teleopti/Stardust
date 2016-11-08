using System;
using System.Linq;
using NHibernate.Util;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader
{
	[DatabaseTest]
	[TestFixture]
	public class ForExcludingStateGroupsForTeamAndSkill
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldLoadForTeamsAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam()
				.WithAgent("expected")
				.WithSkill("phone")
				;

			var teamId = Database.CurrentTeamId();
			var expected = Database.PersonIdFor("expected");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { teamId }, new[] { skillId }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadTop50InAlarmForTeamAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam();
			var team = Database.CurrentTeamId();
			Enumerable.Range(1, 51)
				.ForEach(i =>
				{
					Database
						.WithAgent(i.ToString())
						.WithSkill("phone")
						;
					var current = Database.PersonIdFor(i.ToString());
					WithUnitOfWork.Do(() =>
					{
						Groupings.UpdateGroupingReadModel(new[] { current });
						StatePersister.Persist(new AgentStateReadModelForTest
						{
							PersonId = current,
							TeamId = team,
							AlarmStartTime = "2016-11-07 08:00".Utc(),
							IsRuleAlarm = true,
							StateGroupId = Guid.NewGuid()
						});
					});
				});
			var skillId = Database.SkillIdFor("phone");

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { team }, new[] { skillId }, new Guid?[] { null }))
				.Select(x => x.PersonId).Distinct()
				.Should().Have.Count.EqualTo(50);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForTeamsAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var teamId = Database.CurrentTeamId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { teamId }, new[] { phone, email }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadInAlarmForTeamAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.WithAgent("notInAlarm")
				.WithSkill("phone")
				.WithAgent("expected")
				.WithSkill("phone")
				;
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var notInAlarm = Database.PersonIdFor("notInAlarm");
			var expected = Database.PersonIdFor("expected");
			var team = Database.CurrentTeamId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, wrongTeam, wrongSkill, notInAlarm });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = team,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					TeamId = team,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = notInAlarm,
					TeamId = team,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { team }, new[] { currentSkillId }, new Guid?[] { null }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadForPreviousSkill()
		{
			Now.Is("2016-11-08 08:10");
			Database
				.WithTeam()
				.WithPerson("agent1")
				.WithPersonPeriod("2016-11-01".Date())
				.WithSkill("email")
				.WithPersonPeriod("2016-11-07".Date())
				.WithSkill("phone")
				.WithPersonPeriod("2016-11-10".Date())
				.WithSkill("email")
				;
			var team = Database.CurrentTeamId();
			var personId = Database.CurrentPersonId();
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { personId });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = personId,
					TeamId = team,
					AlarmStartTime = "2016-11-08 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { team }, new[] { email }, new Guid?[] { null }))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldLoadForTeamsAndSkillWhenExcludingNull()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithAgent("wrongState")
				.WithSkill("phone")
				.WithAgent("nullState")
				.WithSkill("phone")
				;

			var teamId = Database.CurrentTeamId();
			var expected = Database.PersonIdFor("expected");
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var wrongState = Database.PersonIdFor("wrongState");
			var nullState = Database.PersonIdFor("nullState");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, wrongState, nullState, wrongTeam });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = wrongState,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true,
					StateGroupId = loggedOut
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = nullState,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = null
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = false,
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { teamId }, new[] { skillId }, new Guid?[] { loggedOut, null }))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadForTeamsAndSkillWhenIncludingNull()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongState")
				.WithSkill("phone")
				;

			var teamId = Database.CurrentTeamId();
			var wrongState = Database.PersonIdFor("wrongState");
			var expected = Database.PersonIdFor("expected");
			var skillId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { wrongState, expected });
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = wrongState,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = loggedOut
				});
				StatePersister.Persist(new AgentStateReadModelForTest
				{
					PersonId = expected,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					StateGroupId = null
				});
			});

			WithUnitOfWork.Get(() => Target.LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(new[] { teamId }, new[] { skillId }, new Guid?[] { loggedOut }))
				.Single().PersonId.Should().Be(expected);
		}
	}
}