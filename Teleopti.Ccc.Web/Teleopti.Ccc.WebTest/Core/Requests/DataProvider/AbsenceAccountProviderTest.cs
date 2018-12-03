using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class AbsenceAccountProviderTest
	{
		[Test]
		public void ShouldRetrieveAbsenceAccountForCurrentUserAndAbsenceAndDate()
		{
			var repository = MockRepository.GenerateMock<IPersonAbsenceAccountRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var personAccountCollection = MockRepository.GenerateMock<IPersonAccountCollection>();
			var person = new Person();
			var absence = MockRepository.GenerateMock<IAbsence>();
			var date = new DateOnly(DateTime.Today);
			var absenceAccount = new AccountDay(date);
			var target = new AbsenceAccountProvider(loggedOnUser, repository);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			
			repository.Stub(x => x.Find(person, absence)).Return(personAccountCollection);
			personAccountCollection.Stub(x => x.Find(absence, date)).Return(absenceAccount);

			var result = target.GetPersonAccount(absence, date);

			result.Should().Be.SameInstanceAs(absenceAccount);
		}
	}
}
