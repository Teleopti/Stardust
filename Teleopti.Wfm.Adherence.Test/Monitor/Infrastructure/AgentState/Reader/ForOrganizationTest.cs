using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ForOrganizationTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;
		public MutableNow Now;
		public ICurrentUnitOfWork UnitOfWork;

		[Test]
		public void ShouldLoadStatesInAlarmOnly()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = Guid.NewGuid(),
				AlarmStartTime = null,
				IsRuleAlarm = false
			});

			var result = Target.Read(new AgentStateFilter()
			{
				TeamIds = teamId.AsArray(),
				InAlarm = true
			});

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirst()
		{
			Now.Is("2016-06-15 12:00");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 11:30".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 11:00".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.Read(new AgentStateFilter()
			{
				TeamIds = teamId.AsArray(),
				InAlarm = true
			});

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
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = Guid.NewGuid(),
				AlarmStartTime = null,
				IsRuleAlarm = false
			});

			var result = Target.Read(new AgentStateFilter()
			{
				SiteIds = siteId.AsArray(),
				InAlarm = true
			});

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadWithStateGroupId()
		{
			Now.Is("2016-06-15 12:00");
			var site = Guid.NewGuid();
			var person = Guid.NewGuid();
			var phoneState = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = person,
				SiteId = site,
				AlarmStartTime = "2016-06-15 12:00".Utc(),
				IsRuleAlarm = true,
				StateGroupId = phoneState
			});

			Target.Read(new AgentStateFilter()
				{
					SiteIds = site.AsArray(),
					InAlarm = true
				})
				.Single().StateGroupId.Should().Be(phoneState);
		}

		[Test]
		public void ShouldLoadStatesOrderByLongestAlarmTimeFirstForSites()
		{
			Now.Is("2016-06-15 12:00");
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 11:30".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				SiteId = siteId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 11:00".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.Read(new AgentStateFilter()
			{
				SiteIds = siteId.AsArray(),
				InAlarm = true
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldLoadSchedule()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
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

			var result = Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()}).Single();

			result.Shift.Single().Color.Should().Be(Color.Green.ToArgb());
			result.Shift.Single().StartTime.Should().Be("2016-05-30 08:00".Utc());
			result.Shift.Single().EndTime.Should().Be("2016-05-30 09:00".Utc());
			result.Shift.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldLoadOutOfAdherences()
		{
			var teamId = Guid.NewGuid();
			Persister.UpsertWithState(new AgentStateReadModelForTest
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

			var outOfAdherence = Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()}).Single()
				.OutOfAdherences.Single();
			outOfAdherence.StartTime.Should().Be("2016-06-16 08:00".Utc());
			outOfAdherence.EndTime.Should().Be("2016-06-16 08:10".Utc());
		}

		[Test]
		public void ShouldLoadNullJsonValues()
		{
			var teamId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId,
			});
			UnitOfWork.Current().FetchSession()
				.CreateSQLQuery("UPDATE [ReadModel].[AgentState] SET Shift = NULL, OutOfAdherences = NULL")
				.ExecuteUpdate();

			Assert.DoesNotThrow(() => Target.Read(new AgentStateFilter {TeamIds = teamId.AsArray()}));
		}
	}
}