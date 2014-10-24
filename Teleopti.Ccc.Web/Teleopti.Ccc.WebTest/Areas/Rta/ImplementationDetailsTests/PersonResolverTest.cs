using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Resolvers;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests
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

			databaseReader.Stub(d => d.ExternalLogOns())
			              .Return(new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>(dictionary));

			IEnumerable<PersonWithBusinessUnit> resolvedList;
			target.TryResolveId(4, "1234", out resolvedList);
			resolvedList.Single().PersonId.Should().Be.EqualTo(personId);
		}
	}
}
