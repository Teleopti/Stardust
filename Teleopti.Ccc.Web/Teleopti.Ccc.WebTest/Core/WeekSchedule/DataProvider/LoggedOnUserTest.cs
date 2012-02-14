using System;
using System.Security.Principal;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
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
		private IPrincipal previousPrincipal;

		[TearDown]
		public void Teardown() { Thread.CurrentPrincipal = previousPrincipal; }

		private void SetupPrincipal(IPerson person)
		{
			previousPrincipal = Thread.CurrentPrincipal;
			Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("name", null, null, null), person);
		}

		[Test]
		public void ShouldGetCurrentPersonFromPrincipal()
		{
			var person = PersonFactory.CreatePerson();
			SetupPrincipal(person);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository);

			var result = target.CurrentUser();

			Assert.That(result, Is.SameAs(person));
		}

		[Test]
		public void ShouldGetMyTeam()
		{
			var person = PersonFactory.CreatePerson();
			var team = new Team();
			person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(DateOnly.Today, team));
			SetupPrincipal(person);
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			personRepository.Stub(x => x.Get(Arg<Guid>.Is.NotNull)).Return(person);
			var target = new LoggedOnUser(personRepository);

			var result = target.MyTeam(DateOnly.Today);

			result.Should().Be.EqualTo(team);
		}
	}
}