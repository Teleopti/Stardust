using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[DatabaseTest]
	[TestFixture]
	public class ForCombinationsTest
	{
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldExcludeDeletedAgentsForSiteAndTeam()
		{
			Now.Is("2016-11-07 08:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId
				});

				StatePersister.UpsertNoAssociation(personId1);
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter
					{
						SiteIds = siteId.AsArray(),
						TeamIds = teamId.AsArray()
					}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {personId2});
		}

		[Test]
		public void ShouldExcludeDeletedAgentsForSiteAndTeam2()
		{
			Now.Is("2016-11-07 08:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId
				});
				StatePersister.UpsertNoAssociation(personId1);
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter
					{
						SiteIds = siteId.AsArray(),
						TeamIds = teamId.AsArray()
					})).Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {personId2});
		}

		[Test]
		public void ShouldLoadForSiteAndTeam()
		{
			Now.Is("2016-11-07 08:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(new[] {personId1, personId2});
		}

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
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid()
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray(),
					SkillIds = phoneId.AsArray()
				}))
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam);
		}

		[Test]
		public void ShouldLoadInAlarmForSiteAndTeam()
		{
			Now.Is("2016-11-24 08:10");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					SiteId = siteId,
				});
				StatePersister.UpdateState(new AgentStateReadModelForTest
				{
					PersonId = personId1,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					TeamId = teamId,
				});
				StatePersister.UpdateState(new AgentStateReadModelForTest
				{
					PersonId = personId2,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.Upsert(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
				});
				StatePersister.UpdateState(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray(),
					InAlarm = true
				}))
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
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = expectedForSite,
					SiteId = siteId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = expectedForTeam,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = notInAlarm,
					SiteId = siteId,
					TeamId = teamId
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongSkill,
					SiteId = siteId,
					TeamId = teamId,
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongSite,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
				StatePersister.UpsertWithState(new AgentStateReadModelForTest
				{
					PersonId = wrongTeam,
					TeamId = Guid.NewGuid(),
					AlarmStartTime = "2016-11-24 08:00".Utc()
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()
				{
					SiteIds = siteId.AsArray(),
					TeamIds = teamId.AsArray(),
					SkillIds = phoneId.AsArray(),
					InAlarm = true
				})
				.Select(x => x.PersonId)
				.Should().Have.SameValuesAs(expectedForSite, expectedForTeam));
		}
	}
}