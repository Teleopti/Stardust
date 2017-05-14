using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.AgentStateReadModelReader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ForOrganization
	{
		public IJsonSerializer Serializer;
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;
		public MutableNow Now;
		public ICurrentUnitOfWork UnitOfWork;

		[Test]
		public void ShouldLoadAgentStateByTeamId()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId
			});

			var result = Target.ReadFor(null, new [] { teamId}, null);

			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamId()
		{
			var teamId = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = teamId, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = teamId, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.ReadFor(null, new[] { teamId }, null);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesBySiteIds()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { SiteId = siteId1, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { SiteId = siteId2, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { SiteId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.ReadFor(new[] { siteId1, siteId2 }, null, null);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadAgentStatesByTeamIds()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = teamId1, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = teamId2, PersonId = Guid.NewGuid() });
			Persister.PersistWithAssociation(new AgentStateReadModelForTest { TeamId = Guid.Empty, PersonId = Guid.NewGuid() });

			var result = Target.ReadFor(null, new[] { teamId1, teamId2 }, null);

			result.Count().Should().Be(2);
		}

		[Test]
		public void ShouldLoadStatesInAlarmOnly()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = Guid.NewGuid(),
				AlarmStartTime = null,
				IsRuleAlarm = false
			});

			var result = Target.ReadInAlarmFor(null, new[] { teamId }, null);

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirst()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 11:30".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 11:00".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.ReadInAlarmFor(null, new[] { teamId }, null);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesInAlarmOnlyForSites()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = Guid.NewGuid(),
				AlarmStartTime = null,
				IsRuleAlarm = false
			});

			var result = Target.ReadInAlarmFor(new[] { siteId }, null, null);

			result.Single().PersonId.Should().Be(personId1);
		}
		
		[Test]
		public void ShouldLoadWithStateGroupId()
		{
			Now.Is("2016-06-15 12:00");
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			var phoneState = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				PersonId = person,
				SiteId = site,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = phoneState
			});

			Target.ReadInAlarmFor(new[] { site }, null, null)
				.Single().StateGroupId.Should().Be(phoneState);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirstForSites()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 11:30".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 11:00".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.ReadInAlarmFor(new[] { siteId }, null, null);

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadScheduleForTeam()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						Color = Color.Green.ToArgb(),
						StartTime = "2016-05-30 08:00".Utc(),
						EndTime = "2016-05-30 09:00".Utc(),
						Name = "Phone"
					}
				}
			});

			var result = Target.ReadFor(null, new[] { teamId }, null).Single();

			result.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
			result.Shift.Single().StartTime.Should().Be("2016-05-30 08:00".Utc());
			result.Shift.Single().EndTime.Should().Be("2016-05-30 09:00".Utc());
			result.Shift.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldLoadOutOfAdherences()
		{
			var teamId = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 08:00".Utc(),
						EndTime = "2016-06-16 08:10".Utc()
					}
				}
			});

			var outOfAdherence = Target.ReadFor(null, new []{ teamId }, null).Single()
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}

		[Test]
		public void ShouldLoadNullJsonValues()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId,
			});
			UnitOfWork.Current().FetchSession()
				.CreateSQLQuery("UPDATE [ReadModel].[AgentState] SET Shift = NULL, OutOfAdherences = NULL")
				.ExecuteUpdate();

			Assert.DoesNotThrow(() => Target.ReadFor(null, new []{ teamId }, null));
		}

	}
}
