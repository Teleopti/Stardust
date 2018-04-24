using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Authentication
{
	[TestFixture]
	public class LoggedOnUserTest
	{
		[Test]
		public void ShouldGetCurrentPersonFromPrincipal()
		{
			var person = PersonFactory.CreatePerson();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository, fakeCurrentTeleoptiPrincipal(person));

			var result = target.CurrentUser();

			Assert.That(result, Is.SameAs(person));
		}

		[Test]
		public void ShouldReturnNullIfNoPrincipal()
		{
			var principal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			principal.Stub(x => x.Current()).Return(null);
			var person = PersonFactory.CreatePerson();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository, principal);

			var result = target.CurrentUser();

			result.Should().Be.Null();
		}

		private static ICurrentTeleoptiPrincipal fakeCurrentTeleoptiPrincipal(IPerson person)
		{
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("name", null, null, null, null), person);
			return new FakeCurrentTeleoptiPrincipal(principal);
		}
	}
}