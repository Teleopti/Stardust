using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class LoggedOnUserTest
	{
		private ICurrentTeleoptiPrincipal FakeCurrentTeleoptiPrincipal(IPerson person)
		{
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("name", null, null, null, AuthenticationTypeOption.Unknown), person);
			return new FakeCurrentTeleoptiPrincipal(principal);
		}

		[Test]
		public void ShouldGetCurrentPersonFromPrincipal()
		{
			var person = PersonFactory.CreatePerson();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository, FakeCurrentTeleoptiPrincipal(person));

			var result = target.CurrentUser();

			Assert.That(result, Is.SameAs(person));
		}

		[Test]
		public void ShouldGetMyTeam()
		{
			var person = PersonFactory.CreatePerson();
			var team = new Team();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));
			FakeCurrentTeleoptiPrincipal(person);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository, FakeCurrentTeleoptiPrincipal(person));

			var result = target.MyTeam(DateOnly.Today);

			result.Should().Be.EqualTo(team);
		}
	}
}