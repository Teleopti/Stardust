using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{
	[UnitOfWorkTest]
	[TestFixture]
	public class OrderTest
	{
		public Database Database;
		public IAgentStateReadModelPersister Persister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldOrderByAlarmStartTimeForStatesInAlarm()
		{
			Now.Is("2016-06-15 12:05");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.Upsert(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:02".Utc(),
				IsRuleAlarm = true
			});
			Persister.Upsert(new AgentStateReadModelForTest
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
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var person3 = Guid.NewGuid();
			
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person1,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person2,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person3,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			
			Persister.UpsertName(person1, "Ashley", "Andeen");
			Persister.UpsertName(person2, "Alina", "Andeen");
			Persister.UpsertName(person3, "Pierre", "Baldi");

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = new[]{"FirstName", "LastName"},
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(person3);
			result.Second().PersonId.Should().Be(person1);
			result.Last().PersonId.Should().Be(person2);
		}

		[Test]
		public void ShouldOrderByTeamName()
		{
			
			Persister.Upsert(new AgentStateReadModelForTest
			{
				SiteName = "aSite",
				TeamName = "aTeam",
				PersonId = Guid.NewGuid()
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				SiteName = "aSite",
				TeamName = "cTeam",
				PersonId = Guid.NewGuid()
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				SiteName = "aSite",
				TeamName = "bTeam",
				PersonId = Guid.NewGuid()
			});
	
			var result = Target.Read(new AgentStateFilter()
			{
				OrderBy = new[]{"SiteName", "TeamName"},
				Direction = "desc"
			});

			result.First().TeamName.Should().Be("cTeam");
			result.Second().TeamName.Should().Be("bTeam");
			result.Last().TeamName.Should().Be("aTeam");
		}
		
		[Test]
		public void ShouldOrderByStateName()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Phone"
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = new[]{"StateName"},
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(personId2);
		}
		
		[Test]
		public void ShouldOrderByStateStartTime()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				StateStartTime = "2017-11-03 11:30".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				StateStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId3,
				StateName = "Phone",
				StateStartTime = "2017-11-03 11:45".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = new[]{"StateStartTime"},
				Direction = "asc"
			});

			result.First().PersonId.Should().Be(personId1);
			result.Second().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId3);
		}	
		
		[Test]
		public void ShouldOrderByAlarmStartTime()
		{
			Now.Is("2017-11-03 12:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "Email",
				AlarmStartTime = "2017-11-03 11:30".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "Lunch",
				AlarmStartTime = "2017-11-03 11:15".Utc(),
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId3,
				StateName = "Phone",
				AlarmStartTime = "2017-11-03 11:45".Utc(),
			});

			var result = Target.Read(new AgentStateFilter
			{
				OrderBy = new[]{"AlarmStartTime"},
				Direction = "desc"
			});

			result.First().PersonId.Should().Be(personId3);
			result.Second().PersonId.Should().Be(personId2);
			result.Last().PersonId.Should().Be(personId1);
		}

	}
}