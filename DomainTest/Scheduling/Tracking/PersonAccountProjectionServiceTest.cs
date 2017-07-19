using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    [TestFixture]
    public class PersonAccountProjectionServiceTest
    {
	    [Test]
	    public void VerifyNumberOfDays()
	    {
		    var person = PersonFactory.CreatePerson();
		    var absence = AbsenceFactory.CreateAbsence("for test");
		    var personAbsenceAccount = new PersonAbsenceAccount(person, absence);

		    var firstAccount = new AccountDay(new DateOnly(2001, 1, 1));
		    var secondAccount = new AccountDay(new DateOnly(2001, 1, 2));
		    personAbsenceAccount.Add(firstAccount);
		    personAbsenceAccount.Add(secondAccount);
			
		    var scenario = ScenarioFactory.CreateScenarioAggregate();
		    var storage = new FakeScheduleStorage();
		    storage.Add(new PersonAbsence(person, scenario,
			    new AbsenceLayer(absence,
				    firstAccount.StartDate.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()))));

		    var target = new PersonAccountProjectionService(firstAccount);
		    var days = target.CreateProjection(storage, scenario);

		    //Verify correct number of days is returned
		    Assert.AreEqual(1, days.Count);
	    }
    }
}
