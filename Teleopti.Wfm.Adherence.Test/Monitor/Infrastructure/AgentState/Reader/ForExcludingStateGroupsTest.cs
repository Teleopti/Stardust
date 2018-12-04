using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[DatabaseTest]
	[TestFixture]
	public class ForExcludingStateGroupsTest
	{
		public IGroupingReadOnlyRepository Groupings;
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldLoadForSite()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					ExcludedStateIds = loggedOut.AsNullableArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldLoadForTeam()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					TeamIds = teamId.AsArray(),
					ExcludedStateIds = loggedOut.AsNullableArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldLoadForSiteAndTeam()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray(),
					ExcludedStateIds = loggedOut.AsNullableArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1, personId2);
		}

		[Test]
		public void ShouldLoadForSiteTeamAndSkill()
		{
			Now.Is("2016-11-28 08:10");
			Database
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("excludedState")
				.WithSkill("phone")
				.WithAgent("expectedForSite")
				.WithSkill("phone")
				.WithAgent("expectedForTeam")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var expectedForSite = Database.PersonIdFor("expectedForSite");
			var expectedForTeam = Database.PersonIdFor("expectedForTeam");
			var excludedState = Database.PersonIdFor("excludedState");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var siteId = Database.CurrentSiteId();
			var teamId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = excludedState,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray(),
					SkillIds = phoneId.AsArray(),
					ExcludedStateIds = loggedOut.AsNullableArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam);
		}

		[Test]
		public void ShouldLoadForSkill()
		{
			Now.Is("2016-11-28 08:10");
			Database
				.WithPerson("expected")
				.WithPersonPeriod("2016-01-01")
				.WithSkill("email")
				.WithPersonPeriod("2016-06-01")
				.WithSkill("phone")
				.WithPersonPeriod("2017-01-01")
				.WithSkill("email")
				.WithAgent("wrongState")
				.WithSkill("phone")
				.UpdateGroupings()
				;
			var expected = Database.PersonIdFor("expected");
			var wrongState = Database.PersonIdFor("wrongState");
			var phoneId = Database.SkillIdFor("phone");
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = expected,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongState,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
					new AgentStateFilter()
					{
						SkillIds = phoneId.AsArray(),
						ExcludedStateIds = loggedOut.AsNullableArray()
					}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expected);
		}

		[Test]
		public void ShouldLoadNullStateGroups()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = null
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					ExcludedStateIds = loggedOut.AsNullableArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldExcludeNullStateGroup()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc(),
					StateGroupId = null
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					ExcludedStateIds = new Guid?[] {null}
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}

		[Test]
		public void ShouldExcludeNullAndRegularStateGroup()
		{
			Now.Is("2016-11-28 08:10");
			var personId1 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-28 08:00".Utc(),
					StateGroupId = Guid.NewGuid()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-28 08:00".Utc(),
					StateGroupId = null
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					SiteId = siteId,
					AlarmStartTime = "2016-11-28 08:00".Utc(),
					StateGroupId = loggedOut
				});
			});

			WithUnitOfWork.Get(() => Target.Read(
				new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					ExcludedStateIds = new Guid?[]
					{
						null,
						loggedOut
					}
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1);
		}



		/*
		[Test]
		public void ShouldLoadForSiteTeamAndSkill()
		{
			Now.Is("2016-11-07 08:00");
			Database
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("expectedForSite")
				.WithSkill("phone")
				.WithAgent("expectedForTeam")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var expectedForSite = Database.PersonIdFor("expectedForSite");
			var expectedForTeam = Database.PersonIdFor("expectedForTeam");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var siteId = Database.CurrentSiteId();
			var teamId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] {siteId}, new []{teamId}, new[] {phoneId}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam);
		}


		[Test]
		public void ShouldInAlarmLoadForSiteAndTeam()
		{
			Now.Is("2016-11-24 08:10");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadFor(new[] { siteId }, new[] { teamId }, null))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(personId1, personId2);
		}


		[Test]
		public void ShouldLoadInAlarmForSiteTeamAndSkill()
		{
			Now.Is("2016-11-28 08:10");
			Database
				.WithAgent("wrongSite")
				.WithSkill("phone")
				.WithSite()
				.WithAgent("wrongTeam")
				.WithSkill("phone")
				.WithTeam()
				.WithAgent("notInAlarm")
				.WithSkill("phone")
				.WithAgent("expectedForSite")
				.WithSkill("phone")
				.WithAgent("expectedForTeam")
				.WithSkill("phone")
				.WithAgent("wrongSkill")
				.WithSkill("email")
				.UpdateGroupings()
				;
			var expectedForSite = Database.PersonIdFor("expectedForSite");
			var expectedForTeam = Database.PersonIdFor("expectedForTeam");
			var notInAlarm = Database.PersonIdFor("notInAlarm");
			var wrongSkill = Database.PersonIdFor("wrongSkill");
			var wrongSite = Database.PersonIdFor("wrongSite");
			var wrongTeam = Database.PersonIdFor("wrongTeam");
			var siteId = Database.CurrentSiteId();
			var teamId = Database.CurrentSiteId();
			var phoneId = Database.SkillIdFor("phone");
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = notInAlarm,
					SiteId = siteId,
					TeamId = teamId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.ReadInAlarmFor(new[] { siteId }, new[] { teamId }, new[] { phoneId }))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam);
		}
		*/
	}
}