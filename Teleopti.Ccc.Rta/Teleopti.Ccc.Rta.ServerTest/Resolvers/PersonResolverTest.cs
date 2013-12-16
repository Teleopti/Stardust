using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Resolvers;

namespace Teleopti.Ccc.Rta.ServerTest.Resolvers
{
	[TestFixture]
	public class PersonResolverTest
	{
		[Test]
		public void TryResolveId()
		{
			var businessUnitId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var databaseReader = MockRepository.GenerateStub<IDatabaseReader>();
			var target = new PersonResolver(databaseReader);
			var personWithBusinessUnits = new PersonWithBusinessUnit {BusinessUnitId = businessUnitId, PersonId = personId};
			var dictionary = new Dictionary<string, IEnumerable<PersonWithBusinessUnit>>
				{
					{"4|1234", new[] {personWithBusinessUnits}}
				};

			databaseReader.Stub(d => d.LoadAllExternalLogOns())
			              .Return(new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>(dictionary));

			IEnumerable<PersonWithBusinessUnit> resolvedList;
			target.TryResolveId(4, "1234", out resolvedList);
			resolvedList.Single().PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void TryResolveId_EmptyLogOn_ReturnEmptyString()
		{
			var databaseReader = MockRepository.GenerateStub<IDatabaseReader>();
			var target = new PersonResolver(databaseReader);
			databaseReader.Stub(d => d.LoadAllExternalLogOns())
			              .Return(new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>());

			IEnumerable<PersonWithBusinessUnit> resolvedList;
			target.TryResolveId(4, string.Empty, out resolvedList).Should().Be(true);
			resolvedList.Count(p => p.BusinessUnitId == Guid.Empty && p.PersonId == Guid.Empty).Should().Be(1);
		}
	}
}
