using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{
	[UnitOfWorkTest]
	[TestFixture]
	public class TextFilterTest
	{
		public IAgentStateReadModelPersister Persister;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldFilterOnActivity()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				Activity = "I am on the Phone and stuff"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				Activity = "I am out for lunch having tacos"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Phone"
			});

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldFilterOnActivityWithSite()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.Update(new AgentStateReadModelForTest
			{
				PersonId = person,
				Activity = "Grounded"
			});
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteName = "Lost in space"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Ground"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnState()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				StateName = "A lot of Phone stuff"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				StateName = "A lot of food n stuff"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Phone"
			});

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldFilterOnRule()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				RuleName = "I am not on the Phone but I should be"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				RuleName = "I am not on Lunch but I should be"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Phone"
			});

			result.Single().PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldFilterOnSite()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteName = "Lost in space"
			});
			Persister.Update(new AgentStateReadModelForTest
			{
				PersonId = person,
				Activity = "Grounded"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Lost"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnSite2()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteName = "Lost in space"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Lost"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnSiteWithActivity()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.Update(new AgentStateReadModelForTest
			{
				PersonId = person,
				Activity = "Grounded"
			});
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteName = "Lost in space"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Lost"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnFirstName()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.UpsertName(person, "Pierre", "Baldi");

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "err"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnLastName()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.UpsertName(person, "Pierre", "Boldi");

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "old"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnEmploymentNumber()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value
			});
			Persister.UpsertEmploymentNumber(person, "fortytwo");

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "tyt"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnTeam()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				TeamName = "A-team"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "team"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldFilterOnSiteAfterChange()
		{
			var person = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteId = siteId,
				SiteName = ""
			});
			Persister.UpdateSiteName(siteId, "somewhere");

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "somewhere"
			});

			result.Single().PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldSeparateWords()
		{
			var person = Guid.NewGuid();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person,
				BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value,
				SiteName = "a",
				TeamName = "b"
			});
			Persister.UpsertName(person, "a", "b");
			Persister.UpsertEmploymentNumber(person, "a");
			Persister.Update(new AgentStateReadModelForTest
			{
				PersonId = person,
				RuleName = "b",
				StateName = "a",
				Activity = "b"
			});

			Target.Read(new AgentStateFilter {TextFilter = "ab"}).Should().Be.Empty();
			Target.Read(new AgentStateFilter {TextFilter = "ba"}).Should().Be.Empty();
			Target.Read(new AgentStateFilter {TextFilter = "aa"}).Should().Be.Empty();
			Target.Read(new AgentStateFilter {TextFilter = "bb"}).Should().Be.Empty();
		}

		[Test]
		public void ShouldFilterOnTwoWords()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteName = "Hoth",
				TeamName = "Rebels"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				SiteName = "Hoth",
				TeamName = "Troopers"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Rebels  		Hoth"
			});

			result.Single().PersonId.Should().Be(personId1);
		}
		
		[Test]
		public void ShouldFilterOnTwoWordsSeperatedByTab()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId1,
				SiteName = "Hoth",
				TeamName = "Rebels"
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId2,
				SiteName = "Hoth",
				TeamName = "Troopers"
			});

			var result = Target.Read(new AgentStateFilter
			{
				TextFilter = "Rebels	Hoth"
			});

			result.Single().PersonId.Should().Be(personId1);
		}
	}
}