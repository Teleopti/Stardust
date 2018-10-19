using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Search;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Search
{
	[DomainTest]
	public class PersonSearchProviderTest
	{
		public IPersonRepository PersonRepository;
		public PersonSearchProvider Target;

		[Test]
		public void PersonSearchShouldReturnPersonsMatchingKeyword()
		{
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John","Doe")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("A", "B")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("C", "D")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("E", "F")));

			Target.FindPersonsByKeywords("John").Count.Should().Be(1);
		}

		[Test]
		public void PersonSearchShouldReturnPersonsMatchingKeywords()
		{
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John", "Doe")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("A", "B")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("C", "D")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("E", "F")));

			Target.FindPersonsByKeywords("John Doe").Count.Should().Be(1);
		}

		[Test]
		public void PersonSearchShouldReturnSeveralPersonsOnMatch()
		{
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John", "Doe")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("A", "John")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("Jon", "A")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("EJohn", "F")));

			Target.FindPersonsByKeywords("John").Count.Should().Be(3);
		}

		[Test]
		public void PersonSearchShouldSortOnKeywordRefCount()
		{
			var nonSortedPerson1 = PersonFactory.CreatePerson(new Name("John", "Dere"));
			var nonSortedPerson2 = PersonFactory.CreatePerson(new Name("Doe", "E"));
			PersonRepository.Add(nonSortedPerson1);
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("C", "B")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John Magnus Extra", "Doe")));
			PersonRepository.Add(nonSortedPerson2);
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John", "Doe")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("E", "B")));

			var result = Target.FindPersonsByKeywords("John Magnus Doe");

			result.Count.Should().Be(4);
			result[0].Name.FirstName.Should().Be("John Magnus Extra");
			result[0].Name.LastName.Should().Be("Doe");

			result[1].Name.FirstName.Should().Be("John");
			result[1].Name.LastName.Should().Be("Doe");

			result.Should().Contain(nonSortedPerson1);
			result.Should().Contain(nonSortedPerson2);
		}

		[Test]
		public void PersonSearchShouldReturnSinglePersonOnPerfectMatch()
		{
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John", "Doe")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("John", "B")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("Doe", "D")));
			PersonRepository.Add(PersonFactory.CreatePerson(new Name("C", "Doe")));

			Target.FindPersonsByKeywords("John Doe").Count.Should().Be(1);
		}
	}
}
