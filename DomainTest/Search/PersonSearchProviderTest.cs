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
	}
}
