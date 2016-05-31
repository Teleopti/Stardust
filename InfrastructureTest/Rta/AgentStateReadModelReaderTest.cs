using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStateReadModelReaderTest
	{
		public IJsonSerializer Serializer;
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldLoadAgentStateByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId
			});

			var result = Target.LoadForTeam(teamId);

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamId()
		{
			var teamId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest { TeamId = teamId, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { TeamId = teamId, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { TeamId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.LoadForTeam(teamId);

			result.Count.Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesBySiteIds()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest { SiteId = siteId1, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { SiteId = siteId2, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { SiteId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.LoadForSites(new[] { siteId1, siteId2 }, false);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamIds()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest { TeamId = teamId1, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { TeamId = teamId2, PersonId = Guid.NewGuid() });
			Persister.Persist(new AgentStateReadModelForTest { TeamId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.LoadForTeams(new[] { teamId1, teamId2 }, false);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadStatesInAlarmOnly()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				IsRuleAlarm = true
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = Guid.NewGuid(),
				IsRuleAlarm = false
			});

			var result = Target.LoadForTeams(new[] { teamId }, true);

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirst()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			});

			var result = Target.LoadForSites(new[] { siteId }, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadTeamStatesOrderByLongestAlarmTimeFirst()
		{
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2015-12-16 8:30".Utc()
			});
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2015-12-16 8:00".Utc()
			});

			var result = Target.LoadForTeams(new[] { teamId }, true);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadScheduleForTeam()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						Color = "#80FF80",
						StartTime = "2016-05-30 08:00",
						EndTime = "2016-05-30 09:00"
					}
				}
			});

			var result = Target.LoadForTeam(teamId).Single();

			result.Shift.Single().Color.Should().Be("#80FF80");
			result.Shift.Single().StartTime.Should().Be("2016-05-30 08:00");
			result.Shift.Single().EndTime.Should().Be("2016-05-30 09:00");
		}
	}
}
