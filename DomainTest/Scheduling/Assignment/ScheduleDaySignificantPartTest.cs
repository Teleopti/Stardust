using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class ScheduleDaySignificantPartTest
	{
		[Test]
		public void ShouldHandleAllCases([ValueSource(nameof(significantPartTestCases))] SignificantPartTestCase testCase)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2017, 01, 12));
			var scenario = ScenarioFactory.CreateScenario("Default", true, true);
			var dateOnly = new DateOnly(2017, 01, 13);
			var scheduleDay = ScheduleDayFactory.Create(dateOnly, person, scenario);
			testCase.Cases.ForEach(c => c.Action(scheduleDay));
			var result = scheduleDay.SignificantPart();
			result.Should().Be.EqualTo(testCase.ExpectedResult);
		}

		private static DateTimePeriod makePeriod(IScheduleDay scheduleDay, int startHour, int endHour)
		{
			return scheduleDay.DateOnlyAsPeriod.Period().ChangeStartTime(TimeSpan.FromHours(startHour)).ChangeEndTime(TimeSpan.FromHours(endHour-24));
		}
		private static readonly Case empty = new Case ("Empty",scheduleDay => { });

		private static readonly Func<int, int, Case> shift = (start, end) => new Case($"Shift ({start:D2} - {end:D2})", scheduleDay =>
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person, scheduleDay.Scenario,
				ActivityFactory.CreateActivity("Phone"), makePeriod(scheduleDay, start, end), ShiftCategoryFactory.CreateShiftCategory("Normal")))
		);

		private static readonly Func<int, int, Case> absence = (start, end) => new Case($"Absence ({start:D2} - {end:D2})", scheduleDay =>
			scheduleDay.Add(PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, makePeriod(scheduleDay, start, end), AbsenceFactory.CreateAbsence("Absence")))
		);

		private static readonly Func<int, int, Case> overtime = (start, end) => new Case($"Overtime ({start:D2} - {end:D2})", scheduleDay =>
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithOvertimeShift(scheduleDay.Person, scheduleDay.Scenario,
				ActivityFactory.CreateActivity("Overtime"), makePeriod(scheduleDay, start, end)))
		);

		private static readonly Func<int, int, Case> personalActivity = (start, end) => new Case($"Personal Activity ({start:D2} - {end:D2})", scheduleDay =>
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithPersonalShift(scheduleDay.Person, scheduleDay.Scenario,
				ActivityFactory.CreateActivity("Personal"), makePeriod(scheduleDay, start, end)))
		);

		private static readonly Case dayOff = new Case("Dayoff", scheduleDay =>
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Person, scheduleDay.Scenario,
				scheduleDay.DateOnlyAsPeriod.DateOnly, DayOffFactory.CreateDayOff(new Description("Dayoff"))))
		);

		private static readonly Case contractDayOff = new Case("Contract Dayoff", scheduleDay =>
		{
			scheduleDay.Person.Period(scheduleDay.DateOnlyAsPeriod.DateOnly)
				.PersonContract.ContractSchedule.AddContractScheduleWeek(ContractScheduleFactory.CreateContractScheduleWithoutWorkDays("AllDaysOff").ContractScheduleWeeks.First());
		});

		private static readonly Case preference = new Case("Preference", scheduleDay =>
			scheduleDay.Add(new PreferenceDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new PreferenceRestriction()))
		);

		private static readonly Case studentAvailability = new Case("Student Availability", scheduleDay =>
			scheduleDay.Add(new StudentAvailabilityDay(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly,
				new List<IStudentAvailabilityRestriction> { new StudentAvailabilityRestriction() }))
		);

		private static SignificantPartTestCase[] significantPartTestCases = {
			new SignificantPartTestCase(empty).Returns(SchedulePartView.None),
			new SignificantPartTestCase(shift(8, 17)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(absence(0, 24)).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(absence(10, 12)).Returns(SchedulePartView.Absence),
			new SignificantPartTestCase(overtime(8, 17)).Returns(SchedulePartView.Overtime),
			new SignificantPartTestCase(personalActivity(8, 17)).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(dayOff).Returns(SchedulePartView.DayOff),
			new SignificantPartTestCase(contractDayOff).Returns(SchedulePartView.None),
			new SignificantPartTestCase(preference).Returns(SchedulePartView.PreferenceRestriction),
			new SignificantPartTestCase(studentAvailability).Returns(SchedulePartView.StudentAvailabilityRestriction),

			new SignificantPartTestCase(shift(10, 12), absence(10, 12)).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(shift(8, 17), absence(0, 24)).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(shift(8, 17), absence(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(19,21)).Returns(SchedulePartView.MainShift),

			new SignificantPartTestCase(shift(8, 17), overtime(8, 17)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), overtime(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), overtime(19, 21)).Returns(SchedulePartView.MainShift),

			new SignificantPartTestCase(shift(8, 17), personalActivity(8, 17)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), personalActivity(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), personalActivity(19, 21)).Returns(SchedulePartView.MainShift),

			new SignificantPartTestCase(shift(8, 17), contractDayOff).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), preference).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), studentAvailability).Returns(SchedulePartView.MainShift),

			new SignificantPartTestCase(absence(0, 24), overtime(8, 17)).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(absence(10, 12), overtime(8, 17)).Returns(SchedulePartView.Overtime),
			new SignificantPartTestCase(absence(19,21), overtime(8, 17)).Returns(SchedulePartView.Overtime),

			new SignificantPartTestCase(absence(0, 24), personalActivity(8, 17)).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(absence(10, 12), personalActivity(8, 17)).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(absence(19,21), personalActivity(8, 17)).Returns(SchedulePartView.PersonalShift),

			new SignificantPartTestCase(absence(0, 24), dayOff).Returns(SchedulePartView.DayOff),
			new SignificantPartTestCase(absence(0, 24), contractDayOff).Returns(SchedulePartView.ContractDayOff),
			new SignificantPartTestCase(absence(0, 24), preference).Returns(SchedulePartView.FullDayAbsence),
			new SignificantPartTestCase(absence(0, 24), studentAvailability).Returns(SchedulePartView.FullDayAbsence),

			new SignificantPartTestCase(personalActivity(8, 17), overtime(8, 17)).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(personalActivity(10, 12), overtime(8, 17)).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(personalActivity(19, 21), overtime(8, 17)).Returns(SchedulePartView.PersonalShift),

			new SignificantPartTestCase(overtime(8, 17), dayOff).Returns(SchedulePartView.Overtime),
			new SignificantPartTestCase(overtime(8, 17), contractDayOff).Returns(SchedulePartView.Overtime),
			new SignificantPartTestCase(overtime(8, 17), preference).Returns(SchedulePartView.Overtime),
			new SignificantPartTestCase(overtime(8, 17), studentAvailability).Returns(SchedulePartView.Overtime),

			new SignificantPartTestCase(personalActivity(8, 17), dayOff).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(personalActivity(8, 17), contractDayOff).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(personalActivity(8, 17), preference).Returns(SchedulePartView.PersonalShift),
			new SignificantPartTestCase(personalActivity(8, 17), studentAvailability).Returns(SchedulePartView.PersonalShift),

			new SignificantPartTestCase(dayOff, contractDayOff).Returns(SchedulePartView.DayOff),
			new SignificantPartTestCase(dayOff, preference).Returns(SchedulePartView.DayOff),
			new SignificantPartTestCase(dayOff, studentAvailability).Returns(SchedulePartView.DayOff),

			new SignificantPartTestCase(contractDayOff, preference).Returns(SchedulePartView.PreferenceRestriction),
			new SignificantPartTestCase(contractDayOff, studentAvailability).Returns(SchedulePartView.StudentAvailabilityRestriction),

			new SignificantPartTestCase(preference, studentAvailability).Returns(SchedulePartView.PreferenceRestriction),

			new SignificantPartTestCase(shift(8, 17),absence(10, 12), personalActivity(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17),absence(10, 12), personalActivity(19, 21)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17),absence(10, 12), personalActivity(13, 15)).Returns(SchedulePartView.MainShift),

			new SignificantPartTestCase(shift(8, 17), absence(10, 12), overtime(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(10, 12), overtime(19, 21)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(10, 12), overtime(13, 15)).Returns(SchedulePartView.MainShift),
													
			new SignificantPartTestCase(shift(8, 17), absence(10, 12), overtime(19, 21), personalActivity(10, 12)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(10, 12), overtime(19, 21), personalActivity(13, 15)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(19, 21), overtime(19, 21), personalActivity(13, 15)).Returns(SchedulePartView.MainShift),
			new SignificantPartTestCase(shift(8, 17), absence(0, 24), overtime(19, 21), personalActivity(13, 15)).Returns(SchedulePartView.FullDayAbsence)
		};
	}

	public class Case
	{
		public string Name { get; set; }
		public Action<IScheduleDay> Action { get; set; }

		public Case(string name, Action<IScheduleDay> action)
		{
			Name = name;
			Action = action;
		}
	}

	public class SignificantPartTestCase
	{
		public readonly List<Case> Cases;
		public SchedulePartView ExpectedResult;

		public SignificantPartTestCase(params Case[] setups)
		{
			Cases = setups.ToList();
		}

		public SignificantPartTestCase Returns(SchedulePartView returns)
		{
			ExpectedResult = returns;
			return this;
		}

		public override string ToString()
		{
			return $"{string.Join(", ", Cases.Select(x => x.Name))} -> {ExpectedResult}";
		}
	}
}