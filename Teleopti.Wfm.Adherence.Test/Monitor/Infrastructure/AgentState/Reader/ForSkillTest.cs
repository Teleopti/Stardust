using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[DatabaseTest]
	[TestFixture]
	public class ForSkillTest
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit CurrentBusinessUnit;

		[Test]
		public void ShouldLoad()
		{
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter
					{
						SkillIds = currentSkillId.AsArray()
					}))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldLoadForMultipleSkills()
		{
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("email");
			var agent1 = Database.PersonIdFor("agent1");
			var agent2 = Database.PersonIdFor("agent2");
			var skill1 = Database.SkillIdFor("phone");
			var skill2 = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {agent1, agent2});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = agent1
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = agent2
				});
			});


			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = new[] {skill1, skill2}}))
				.Count().Should().Be(2);
		}

		[Test]
		public void ShouldNotLoadDuplicatesForMultipleSkills()
		{
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithSkill("email");
			var agent1 = Database.PersonIdFor("agent1");
			var skill1 = Database.SkillIdFor("phone");
			var skill2 = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {agent1});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = agent1
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = new[] {skill1, skill2}}))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldOnlyLoadAgentsForSelectedSkill()
		{
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("email");
			var agent1 = Database.PersonIdFor("agent1");
			var agent2 = Database.PersonIdFor("agent2");
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {agent1, agent2});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = agent1
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = agent2
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = currentSkillId.AsArray()}))
				.Single().PersonId.Should().Be(agent1);
		}

		[Test]
		public void ShouldNotLoadForPreviousSkill()
		{
			Now.Is("2016-06-20");
			Database
				.WithPerson("agent1")
				.WithPersonPeriod("2016-01-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-06-19")
				.WithSkill("phone")
				;
			var personId = Database.CurrentPersonId();
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = email.AsArray()}))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotLoadForFutureSkill()
		{
			Now.Is("2016-06-20");
			Database
				.WithPerson("agent1")
				.WithPersonPeriod("2016-01-01")
				.WithSkill("email")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("phone")
				;
			var personId = Database.CurrentPersonId();
			var phoneId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = phoneId.AsArray()}))
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldLoadStatesInAlarmForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SkillIds = currentSkillId.AsArray(),
					InAlarm = true
				}))
				.Count().Should().Be(1);
		}


		[Test]
		public void ShouldLoadWithStateGroupId()
		{
			Now.Is("2016-09-23 08:00");
			var phoneState = Guid.NewGuid();
			Database
				.WithAgent("agent1")
				.WithSkill("phone");
			var person = Database.PersonIdFor("agent1");
			var skill = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {person});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = person,
					AlarmStartTime = "2016-09-23 07:50".Utc(),
					IsRuleAlarm = true,
					StateGroupId = phoneState
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SkillIds = skill.AsArray(),
					InAlarm = true
				}))
				.Single().StateGroupId.Should().Be(phoneState);
		}

		[Test]
		public void ShouldLoadStatesInAlarmForMultipleSkills()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("email");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var skill1 = Database.SkillIdFor("phone");
			var skill2 = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId1, personId2});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
			});


			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter()
					{
						SkillIds = new[] {skill1, skill2},
						InAlarm = true
					}))
				.Count().Should().Be(2);
		}

		[Test]
		public void ShouldNotLoadDuplicateStatesInAlarmForMultipleSkills()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithSkill("email");
			var personId1 = Database.PersonIdFor("agent1");
			var skill1 = Database.SkillIdFor("phone");
			var skill2 = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId1});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
			});


			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SkillIds = new[] {skill1, skill2},
					InAlarm = true
				}))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldOnlyLoadStatesInAlarmForSkill()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId1, personId2});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId2
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SkillIds = currentSkillId.AsArray(),
					InAlarm = true
				}))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldLoadStatesInAlarmForSkillOrderedByLongestAlarmTime()
		{
			Now.Is("2016-06-20 12:10");
			Database
				.WithAgent("agent1")
				.WithSkill("phone")
				.WithAgent("agent2")
				.WithSkill("phone");
			var personId1 = Database.PersonIdFor("agent1");
			var personId2 = Database.PersonIdFor("agent2");
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId1, personId2});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId1,
					AlarmStartTime = "2016-06-20 12:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId2,
					AlarmStartTime = "2016-06-20 12:01".Utc(),
					IsRuleAlarm = true
				});
			});

			var agents = WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
			{
				SkillIds = currentSkillId.AsArray(),
				InAlarm = true
			}).ToArray());
			agents.First().PersonId.Should().Be(personId1);
			agents.Last().PersonId.Should().Be(personId2);
		}


		[Test]
		public void ShouldLoadOutOfAdherences()
		{
			Database
				.WithAgent()
				.WithSkill("phone");
			var personId = Database.CurrentPersonId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {personId});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = personId,
					OutOfAdherences = new[]
					{
						new AgentStateOutOfAdherenceReadModel
						{
							StartTime = "2016-06-16 08:00".Utc(),
							EndTime = "2016-06-16 08:10".Utc()
						}
					}
				});
			});

			var outOfAdherence =
				WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter {SkillIds = currentSkillId.AsArray()}))
					.Single().OutOfAdherences.Single();

			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}
	}
}