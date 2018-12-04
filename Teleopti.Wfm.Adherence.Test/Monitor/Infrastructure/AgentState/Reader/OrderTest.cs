using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[UnitOfWorkTest]
	[TestFixture]
	public class OrderTest
	{
		public IAgentStateReadModelPersister Persister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit BusinessUnit;

		[Test]
		public void ShouldOrderByAlarmStartTimeForStatesInAlarm()
		{
			Now.Is("2016-06-15 12:05");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:02".Utc(),
				IsRuleAlarm = true
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.Read(new AgentStateFilter()
			{
				TeamIds = teamId.AsArray(),
				InAlarm = true
			});

			result.First().PersonId.Should().Be(personId2);
			result.Second().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldOrderByName()
		{
			var person1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var person2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person1,
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				FirstName = "Pierre",
				LastName = "Baldi"
			});
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person2,
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				FirstName = "Ashley",
				LastName = "Andeen"
			});

			var result = Target.Read(new AgentStateFilter {OrderBy = "Name"});

			result.First().PersonId.Should().Be(person2);
			result.Last().PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldOrderByAscendingByDefault()
		{
			var person1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var person2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person1,
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				FirstName = "Pierre",
				LastName = "Baldi"
			});
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person2,
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				FirstName = "Ashley",
				LastName = "Andeen"
			});

			var result = Target.Read(new AgentStateFilter {OrderBy = "Name", Direction = "hellooou"});

			result.First().PersonId.Should().Be(person2);
			result.Last().PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldOrderDescendantly()
		{
			var pierre = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var ashley = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");

			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = pierre,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				FirstName = "Pierre",
				LastName = "Baldi"
			});

			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = ashley,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				FirstName = "Ashley",
				LastName = "Andeen"
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "Name",
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(pierre);
			result.Last().PersonId.Should().Be(ashley);
		}

		[Test]
		public void ShouldOrderByTeamName()
		{
			var person1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var person2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				SiteName = "aSite",
				TeamName = "bTeam",
				PersonId = person1
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				SiteName = "aSite",
				TeamName = "aTeam",
				PersonId = person2
			});

			var result = Target.Read(new AgentStateFilter()
			{
				OrderBy = "SiteAndTeamName",
				Direction = "asc"
			});

			result.First().TeamName.Should().Be("aTeam");
			result.Last().TeamName.Should().Be("bTeam");
		}

		[Test]
		public void ShouldOrderByStateName()
		{
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Phone"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Lunch"
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "State",
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldOrderByStateStartTime()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				StateStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				StateStartTime = "2017-11-03 11:30".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "TimeInState",
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldOrderByAlarmStartTime()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				AlarmStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				AlarmStartTime = "2017-11-03 11:30".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "TimeInAlarm",
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldOrderByOutOfAdherenceStartTime()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				OutOfAdherenceStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				OutOfAdherenceStartTime = "2017-11-03 11:30".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "TimeOutOfAdherence",
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldOrderByOutOfAdherenceStartTimeWhenNull()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				OutOfAdherenceStartTime = "2017-11-03 11:30".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				OutOfAdherenceStartTime = null,
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "TimeOutOfAdherence",
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldInvertDirection()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				OutOfAdherenceStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.UpsertWithState(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				OutOfAdherenceStartTime = "2017-11-03 11:30".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "TimeOutOfAdherence",
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(personId1);
			result.Last().PersonId.Should().Be(personId2);
		}

		[Test]
		public void ShouldOrderByRuleName()
		{
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				RuleName = "In Adherence"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				RuleName = "Positive"
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = "Rule",
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}
	}
}