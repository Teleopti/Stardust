using NUnit.Framework;
using SharpTestsEx;
using System.Drawing;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	public class ProjectionChangedEventBuilderTest
	{
		public IProjectionChangedEventBuilder Target;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeScenarioRepository ScenarioRepository;
		public IScheduleStorage ScheduleStorage;
		public IScheduleDayProvider ScheduleDayProvider;


		[Test]
		public void ShouldBuildWithCorrectShortNameForFullDayAbsenceWhenScheduleDayHasMoreThanOneFullDayAbsence()
		{

			var date = new DateOnly(2019, 01, 23);
			var schedulePeriod = new DateTimePeriod(2019, 01, 23, 8, 2019, 01, 23, 16);
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);
			var scenario = ScenarioFactory.CreateScenario("default", true, false);
			ScenarioRepository.Has(scenario);


			var personAss = PersonAssignmentFactory.CreateEmptyAssignment(person, scenario, schedulePeriod);
			personAss.AddActivity(ActivityFactory.CreateActivity("activity"), schedulePeriod);
			ScheduleStorage.Add(personAss);
			var scheduleDic = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false, false), schedulePeriod, scenario);

			var scheduleDay = scheduleDic[person].ScheduledDay(date);

			var absenceLayer1 = new AbsenceLayer(AbsenceFactory.CreateAbsence("abs 1", "abs 1", Color.Red).WithId(), new DateTimePeriod(2019, 01, 22, 2019, 01, 24));
			var absenceLayer2 = new AbsenceLayer(AbsenceFactory.CreateAbsence("abs 2", "abs 2", Color.White).WithId(), new DateTimePeriod(2019, 01, 22, 2019, 01, 24));
			scheduleDay.CreateAndAddAbsence(absenceLayer1);
			scheduleDay.CreateAndAddAbsence(absenceLayer2);

			Target.BuildEventScheduleDay(scheduleDay).ShortName.Should().Be.EqualTo("abs 2");

		}

	}
}
