using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
    [DomainTest]
    public class PersonAccountProjectionServiceTest
	{
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IScheduleStorage ScheduleStorage;

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
		    PersonAbsenceRepository.Add(new PersonAbsence(person, scenario,
			    new AbsenceLayer(absence,
				    firstAccount.StartDate.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()))));

		    var target = new PersonAccountProjectionService(firstAccount);
		    var days = target.CreateProjection(ScheduleStorage, scenario);

		    //Verify correct number of days is returned
		    Assert.AreEqual(1, days.Count);
	    }
    }
}
