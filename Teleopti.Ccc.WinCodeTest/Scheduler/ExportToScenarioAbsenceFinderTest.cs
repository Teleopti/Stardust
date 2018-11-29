using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ExportToScenarioAbsenceFinderTest
	{
		private ExportToScenarioAbsenceFinder _target;

		[Test]
		public void ShouldFindAbsences()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target = new ExportToScenarioAbsenceFinder();

				var scenario = ScenarioFactory.CreateScenario("scenario", true, false);
				var person = PersonFactory.CreatePerson();
				var persons = new List<IPerson> {person};
				var dateTimePeriod = new DateTimePeriod(2016, 1, 1, 2016, 1, 2);
				var parameters = new ScheduleParameters(scenario, person, dateTimePeriod);
				var dateOnly = new DateOnly(2016, 1, 1);

				var absence1 = new Absence();
				var absence2 = new Absence();

				var personAbsence1 =
					PersonAbsenceFactory.CreatePersonAbsence(person, scenario, dateTimePeriod, absence1);
				var personAbsence2 =
					PersonAbsenceFactory.CreatePersonAbsence(person, scenario, dateTimePeriod, absence2);

				IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(dateTimePeriod);
				var scheduleDictionary = new ScheduleDictionaryForTest(scenario, scheduleDateTimePeriod,
					new Dictionary<IPerson, IScheduleRange>());
				var range = new ScheduleRange(scheduleDictionary, parameters,
					new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()),
					CurrentAuthorization.Make());
				range.Add(personAbsence1);
				scheduleDictionary.AddTestItem(person, range);

				var scheduleDay = ScheduleDayFactory.Create(dateOnly, person, scenario);
				scheduleDay.Add(personAbsence2);
				var scheduleDaysToExport = new List<IScheduleDay> {scheduleDay};

				var result = _target.Find(scenario, scheduleDictionary, persons, scheduleDaysToExport,
					new List<DateOnly> {dateOnly});
				Assert.IsTrue(result[person].Contains(absence1));
				Assert.IsTrue(result[person].Contains(absence2));
			}
		}
	}
}
	