using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
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
	public class ForOrganizationAndSkillTest
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit CurrentBusinessUnit;

		[Test]
		public void ShouldLoadForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var siteId = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected, wrongSkill, wrongSite});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(
					() => Target.Read(new AgentStateFilter
					{
						SiteIds = siteId.AsArray(),
						SkillIds = currentSkillId.AsArray()
					}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldExcludeDeletedAgentForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("unexpected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var unexpected = Database.PersonIdFor("unexpected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var siteId = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, unexpected, wrongSkill, wrongSite });
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = unexpected,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});

				StatePersister.UpsertNoAssociation(unexpected);
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
				{
					SiteIds = siteId.AsArray(),
					SkillIds = currentSkillId.AsArray()
				}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldExcludeDeletedAgentForSiteAndSkill2()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("unexpected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var unexpected = Database.PersonIdFor("unexpected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var siteId = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, unexpected, wrongSkill, wrongSite });
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = unexpected,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});

				StatePersister.UpsertNoAssociation(unexpected);
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
				{
					SiteIds = siteId.AsArray(),
					SkillIds = currentSkillId.AsArray()
				}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForSites()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var siteId = Database.CurrentSiteId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					SiteId = siteId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter
					{
						SiteIds = siteId.AsArray(),
						SkillIds = new[] {phone, email}
					}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldLoadForTeamAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("expected")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongTeam = Database.PersonIdFor("wrongSite");
			var teamId = Database.CurrentTeamId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected, wrongSkill, wrongTeam});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					TeamId = teamId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					TeamId = teamId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter
					{
						TeamIds = teamId.AsArray(),
						SkillIds = currentSkillId.AsArray()
					}))
				.Single().PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldNotLoadDuplicateRowsForTeams()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("expected")
				.WithSkill("phone")
				.WithSkill("email")
				;
			var expected = Database.PersonIdFor("expected");
			var teamId = Database.CurrentTeamId();
			var phone = Database.SkillIdFor("phone");
			var email = Database.SkillIdFor("email");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] {expected});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					TeamId = teamId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter
					{
						TeamIds = teamId.AsArray(),
						SkillIds = new[] {phone, email}
					}))
				.Single().PersonId.Should().Be(expected);
		}


		[Test]
		public void ShouldLoadInAlarmForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.WithAgent("notInAlarm")
				.WithSkill("phone")
				.WithAgent("expected")
				.WithSkill("phone")
				;
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var notInAlarm = Database.PersonIdFor("notInAlarm");
			var expected = Database.PersonIdFor("expected");
			var site = Database.CurrentSiteId();
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected, wrongSite, wrongSkill, notInAlarm });
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					SiteId = site,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					SiteId = site,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = notInAlarm,
					SiteId = site
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SiteIds = site.AsArray(),
					SkillIds = currentSkillId.AsArray(),
				InAlarm = true
			})
				.Single().PersonId.Should().Be(expected));
		}


		[Test]
		public void ShouldLoadTop50InAlarmForSiteAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithSite();
			var site = Database.CurrentSiteId();
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
						StatePersister.UpsertWithState(new AgentStateReadModelForTest
						{
							BusinessUnitId = CurrentBusinessUnit.CurrentId(),
							PersonId = current,
							SiteId = site,
							AlarmStartTime = "2016-11-07 08:00".Utc(),
							IsRuleAlarm = true
						});
					});
				});
			var skillId = Database.SkillIdFor("phone");

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SiteIds = site.AsArray(),
					SkillIds = skillId.AsArray(),
				InAlarm = true
			})
				.Select(x => x.PersonId).Distinct()
				.Should().Have.Count.EqualTo(50));
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
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					TeamId = team,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = wrongSkill,
					TeamId = team,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = notInAlarm,
					TeamId = team,
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					TeamIds = team.AsArray(),
					SkillIds = currentSkillId.AsArray(),
				InAlarm = true
			})
				.Single().PersonId.Should().Be(expected));
		}


		[Test]
		public void ShouldLoadTop50InAlarmForTeamAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam();
			var teamId = Database.CurrentTeamId();
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
						StatePersister.UpsertWithState(new AgentStateReadModelForTest
						{
							BusinessUnitId = CurrentBusinessUnit.CurrentId(),
							PersonId = current,
							TeamId = teamId,
							AlarmStartTime = "2016-11-07 08:00".Utc(),
							IsRuleAlarm = true
						});
					});
				});
			var skillId = Database.SkillIdFor("phone");

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					TeamIds = teamId.AsArray(),
					SkillIds = skillId.AsArray(),
				InAlarm = true
			})
				.Select(x => x.PersonId).Distinct()
				.Should().Have.Count.EqualTo(50));
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
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { expected });
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = expected,
					TeamId = teamId,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter()
					{
						TeamIds = teamId.AsArray(),
						SkillIds = new[] {phone, email},
						InAlarm = true
					})
				.Single().PersonId.Should().Be(expected));
		}

		[Test]
		public void ShouldLoadInAlarmForMultipleTeamAndSkill()
		{
			Now.Is("2016-11-07 08:10");
			Database
				.WithTeam("A")
				.WithAgent("person1")
				.WithSkill("phone")
				.WithTeam("B")
				.WithAgent("person2")
				.WithSkill("phone")
				;
			var person1 = Database.PersonIdFor("person1");
			var person2 = Database.PersonIdFor("person2");
			var teamA = Database.TeamIdFor("A");
			var teamB = Database.TeamIdFor("B");
			var currentSkillId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				Groupings.UpdateGroupingReadModel(new[] { person1, person2 });
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = person1,
					TeamId = teamA,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					BusinessUnitId = CurrentBusinessUnit.CurrentId(),
					PersonId = person2,
					TeamId = teamB,
					AlarmStartTime = "2016-11-07 08:00".Utc(),
					IsRuleAlarm = true
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter()
					{
						TeamIds = new[] { teamA, teamB },
						SkillIds = currentSkillId.AsArray(),
						InAlarm = true
					})
				.Should().Have.Count.EqualTo(2));
		}
	}
}