using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
	public static class SchedulePartFactory
	{
		public static IList<IScheduleDay> CreateSchedulePartCollection()
		{
			IList<IScheduleDay> schedules = new List<IScheduleDay>();

			IScenario scenario = new Scenario("default");
			scenario.SetId(Guid.NewGuid());

			IPerson person1 = new Person();
			IPerson person2 = new Person();
			IPerson person3 = new Person();

			IShiftCategory shiftCategory1 = new ShiftCategory("ShiftCategory1");
			IShiftCategory shiftCategory2 = new ShiftCategory("ShiftCategory2");

			IList<IAbsence> absenceCollection = AbsenceFactory.CreateAbsenceCollection();
			IAbsence ab1 = absenceCollection[0];
			IAbsence ab2 = absenceCollection[1];
			IAbsence ab3 = absenceCollection[1];

			person1.SetName(new Name("Person", "1"));
			person2.SetName(new Name("Person", "2"));
			person3.SetName(new Name("Person", "3"));
			person1.SetId(Guid.NewGuid());
			person2.SetId(Guid.NewGuid());
			person3.SetId(Guid.NewGuid());

			var period1 = new DateTimePeriod(1800, 1, 1, 1800, 1, 2);
			var period2 = new DateTimePeriod(1800, 1, 2, 1800, 1, 3);
			var period3 = new DateTimePeriod(1800, 1, 3, 1800, 1, 4);

			IPersonAssignment ass1 = CreatePersonAssignment(period1, person1, shiftCategory1, scenario);
			IPersonAssignment ass2 = CreatePersonAssignment(period2, person2, shiftCategory2, scenario);
			IPersonAssignment ass3 = CreatePersonAssignment(period3, person3, shiftCategory2, scenario);

			RaptorTransformerHelper.SetUpdatedOn(ass1, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(ass2, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(ass3, DateTime.Now);

			IPersonAbsence abs1 = CreatePersonAbsence(period3, person1, ab1, scenario);
			IPersonAbsence abs2 = CreatePersonAbsence(period3, person2, ab2, scenario);
			IPersonAbsence abs3 = CreatePersonAbsence(period3, person3, ab3, scenario);

			RaptorTransformerHelper.SetUpdatedOn(abs1, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(abs2, DateTime.Now);
			RaptorTransformerHelper.SetUpdatedOn(abs3, DateTime.Now);

			var currentAuthorization = CurrentAuthorization.Make();
			var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(new DateTimePeriod(1800, 1, 1, 1801, 1, 1)), new PersistableScheduleDataPermissionChecker(currentAuthorization), currentAuthorization);

			IScheduleDay schedulePart1 = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(1800, 1, 1), currentAuthorization);
			IScheduleDay schedulePart11 = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(1800, 1, 2), currentAuthorization);
			IScheduleDay schedulePart111 = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(1800, 1, 3), currentAuthorization);

			IScheduleDay schedulePart2 = ExtractedSchedule.CreateScheduleDay(dic, person2, new DateOnly(1800, 1, 1), currentAuthorization);
			IScheduleDay schedulePart22 = ExtractedSchedule.CreateScheduleDay(dic, person2, new DateOnly(1800, 1, 2), currentAuthorization);
			IScheduleDay schedulePart222 = ExtractedSchedule.CreateScheduleDay(dic, person2, new DateOnly(1800, 1, 3), currentAuthorization);

			IScheduleDay schedulePart3 = ExtractedSchedule.CreateScheduleDay(dic, person3, new DateOnly(1800, 1, 1), currentAuthorization);
			IScheduleDay schedulePart33 = ExtractedSchedule.CreateScheduleDay(dic, person3, new DateOnly(1800, 1, 2), currentAuthorization);
			IScheduleDay schedulePart333 = ExtractedSchedule.CreateScheduleDay(dic, person3, new DateOnly(1800, 1, 3), currentAuthorization);

			schedulePart1.Add(ass1);
			schedulePart111.Add(abs1);

			schedulePart22.Add(ass2);
			schedulePart222.Add(abs2);

			schedulePart333.Add(ass3);
			schedulePart333.Add(abs3);

			schedules.Add(schedulePart1);
			schedules.Add(schedulePart11);
			schedules.Add(schedulePart111);

			schedules.Add(schedulePart2);
			schedules.Add(schedulePart22);
			schedules.Add(schedulePart222);

			schedules.Add(schedulePart3);
			schedules.Add(schedulePart33);
			schedules.Add(schedulePart333);

			return schedules;
		}

		#region test help code

		//create personassignment
		private static IPersonAssignment CreatePersonAssignment(DateTimePeriod period, IPerson person, IShiftCategory shiftCategory, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
												 scenario, Ccc.TestCommon.FakeData.ActivityFactory.CreateActivity("sdfsdf"), period, shiftCategory);
		}

		//create personabsence
		private static PersonAbsence CreatePersonAbsence(DateTimePeriod period, IPerson person, IAbsence ab, IScenario scenario)
		{
			return new PersonAbsence(person, scenario, new AbsenceLayer(ab, period));
		}

		#endregion
	}
}
