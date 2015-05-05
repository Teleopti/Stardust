using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ScheduleDayTest
	{
		private readonly DateTimePeriod rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
		
		[Test]
		public void CanAddTwoLayersWithHoleInBetween()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var start = new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(start));
			var act = new Activity("sdf");
			target.CreateAndAddActivity(act, new DateTimePeriod(start, start.AddHours(1)), new ShiftCategory("sdf"));
			target.CreateAndAddActivity(act, new DateTimePeriod(start.AddHours(4), start.AddHours(5)), new ShiftCategory("sdf"));
			target.PersonAssignment().MainActivities().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void VerifyClone()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			var note = new Note(parameters.Person, new DateOnly(2000, 1, 1), scenario, "The agent is very cute");

			var abs =
				PersonAbsenceFactory.CreatePersonAbsence(parameters.Person, parameters.Scenario,
														 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			target.Add(abs);

			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person,
																	  parameters.Period);

			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description",
				ActivityFactory.CreateActivity("activity"), parameters.Scenario);

			var personMeeting = new PersonMeeting(meeting, new MeetingPerson(parameters.Person, true), rangePeriod);


			personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(parameters.Person, true));

			target.Add(personMeeting);
			target.Add(ass1);
			target.Add(note);

			var clone = (IScheduleDay)target.Clone();
			Assert.AreEqual(target.PersonAbsenceCollection().Count, clone.PersonAbsenceCollection().Count);
			clone.PersonAssignment().Should().Not.Be.Null();
			Assert.AreEqual(target.PersonMeetingCollection().Count, clone.PersonMeetingCollection().Count);
			Assert.AreEqual(target.NoteCollection().Count, clone.NoteCollection().Count);
			Assert.IsFalse(clone.FullAccess);
			Assert.IsFalse(clone.IsFullyPublished);
			Assert.AreEqual(target.OvertimeAvailablityCollection().Count, clone.OvertimeAvailablityCollection().Count);
		}

		[Test]
		public void VerifyProperties()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			Assert.IsFalse(target.FullAccess);
			Assert.IsFalse(target.IsFullyPublished);
			Assert.AreEqual(target.Person.PermissionInformation.DefaultTimeZone().DisplayName, target.TimeZone.DisplayName);

			CollectionAssert.IsEmpty(target.PersonRestrictionCollection());
		}

		[Test]
		public void VerifyCanAdd()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			target.Add(
				PersonAbsenceFactory.CreatePersonAbsence(parameters.Person, parameters.Scenario,
														 new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			Assert.AreEqual(1, target.PersonAbsenceCollection().Count);
		}

		[Test]
		public void VerifyProjectionsAreSorted()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var part = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2000,1,1));
			var ass = new PersonAssignment(person1, scenario, new DateOnly(2000, 1, 1));
			var activity = new Activity("sdf");
			ass.AddActivity(activity, createPeriod(TimeSpan.FromHours(4)));
			ass.AddActivity(activity, createPeriod(TimeSpan.FromHours(1)));
			ass.AddActivity(activity, createPeriod(TimeSpan.FromHours(9)));
			part.Add(ass);

			Assert.AreEqual(new DateTimePeriod(new DateTime(2000, 1, 1, 1, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc)), part.ProjectionService().CreateProjection().Period());
		}

		[Test]
		public void VerifyCreateAndAddDayOff()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			var dayOff = new DayOffTemplate(new Description("test"));
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
			dayOff.Anchor = TimeSpan.FromHours(12);
			target.CreateAndAddDayOff(dayOff);
			target.CreateAndAddDayOff(dayOff);
			target.PersonAssignment().DayOff().Should().Not.Be.Null();
			Assert.AreEqual(
				TimeZoneHelper.ConvertToUtc(target.Period.StartDateTime.Add(dayOff.Anchor),
											target.Person.PermissionInformation.DefaultTimeZone()),
				target.PersonAssignment().DayOff().Anchor);
			Assert.AreEqual(target.Period.StartDateTime, target.PersonAssignment().DayOff().Anchor.Date);
			Assert.AreEqual(new TimeSpan(24, 0, 0), target.PersonAssignment().DayOff().TargetLength);
			Assert.AreEqual(TimeSpan.FromHours(6), target.PersonAssignment().DayOff().Flexibility);
		}


		[Test]
		public void VerifyCreateAndAddAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			var abs =
				PersonAbsenceFactory.CreatePersonAbsence(parameters.Person, parameters.Scenario,
														 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			target.Add(abs);

			IAbsence absence = AbsenceFactory.CreateAbsence("abs");
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 3);
			var absLayer = new AbsenceLayer(absence, period);
			target.Clear<IPersonAbsence>();
			target.CreateAndAddAbsence(absLayer);
			Assert.AreEqual(1, target.PersonAbsenceCollection().Count);
			Assert.AreEqual(period, target.PersonAbsenceCollection()[0].Period);
		}

		[Test]
		public void ShouldCreateAndAddAbsenceWhenAssignmentIsOvernight()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var absencePeriod = new DateTimePeriod(start.AddHours(2), start.AddHours(3));
			var absence = AbsenceFactory.CreateAbsence("abs");
			var absLayer = new AbsenceLayer(absence, absencePeriod);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			target.Add(personAssignment);
			target.CreateAndAddAbsence(absLayer);
			Assert.That(absLayer, Is.EqualTo(target.PersonAbsenceCollection()[0].Layer));
		}

		[Test]
		public void VerifyCreateAndAddNote()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			target.Add(new Note(parameters.Person, new DateOnly(2000, 1, 1), scenario, "The agent is very cute"));

			const string text = "Green haired agent was sent home";
			Assert.IsTrue(target.NoteCollection().Count == 1);
			target.CreateAndAddNote(text);
			Assert.IsTrue(target.NoteCollection().Count == 1);
			Assert.AreEqual(text, target.NoteCollection()[0].GetScheduleNote(new NoFormatting()));
		}

		[Test]
		public void VerifyCreateAndAddActivityMainShiftExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			target.CreateAndAddActivity(activity, period, shiftCategory);
			Assert.AreEqual(1, target.PersonAssignment().MainActivities().Count());
			Assert.AreEqual(period, target.PersonAssignment().MainActivities().First().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityMainShiftOverMidnightExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person,
																	  parameters.Period);
			target.Add(ass1);
			
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			var nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));

			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(target.PersonAssignment(), mainShift);

			target.CreateAndAddActivity(activity, period, shiftCategory);
			Assert.AreEqual(2, target.PersonAssignment().MainActivities().Count());
			Assert.AreEqual(period, target.PersonAssignment().MainActivities().Last().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityNoAssignment()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			target.CreateAndAddActivity(activity, period, shiftCategory);
			Assert.AreEqual(period, target.PersonAssignment().MainActivities().Single().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityNoMainShift()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
				
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var assNoMainShift = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(activity, parameters.Person,
																	  parameters.Period, parameters.Scenario);
			target.Add(assNoMainShift);

			target.CreateAndAddActivity(activity, period, shiftCategory);
			Assert.AreEqual(period, target.PersonAssignment().MainActivities().Single().Period);
		}

		[Test]
		public void ShouldUseExistingPersonAssingmentWhenNoShiftLayers()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 2, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var assNoMainShift = PersonAssignmentFactory.CreateEmptyAssignment(scenario, parameters.Person, parameters.Period);

			target.Add(assNoMainShift);

			target.CreateAndAddActivity(activity, period, shiftCategory);
			Assert.AreEqual(period, target.PersonAssignment().MainActivities().Single().Period);
		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			
			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person,
																	  parameters.Period);

			target.Add(ass1);

			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(1, target.PersonAssignment().PersonalActivities().Count());
			Assert.AreEqual(period, target.PersonAssignment().PersonalActivities().First().Period);

		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentExistsAndAfterMidnight()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person,
																	  parameters.Period);

			target.Add(ass1);

			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(target.PersonAssignment(), mainShift);

			target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(period, target.PersonAssignment().PersonalActivities().Single().Period);
		}

        [Test]
		public void ShouldNotCreateNewAssignmentWhenAdjacentToAssignment()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var shiftCategory = new ShiftCategory("ShiftCategory");
			
			var activity = ActivityFactory.CreateActivity("activity");
			var activity2 = ActivityFactory.CreateActivity("activity2");
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));

			var startAssignment = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            var endAssignment = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
            var periodAssignment = new DateTimePeriod(startAssignment, endAssignment);

			var startPersonalShift = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var endPersonalShift = new DateTime(2000, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var periodPersonalShift = new DateTimePeriod(startPersonalShift, endPersonalShift);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, target.Person, periodAssignment, shiftCategory, parameters.Scenario);
			target.Add(personAssignment);
	        target.CreateAndAddPersonalActivity(activity2, periodPersonalShift);

			target.PersonAssignment().Should().Not.Be.Null();
			Assert.AreEqual(personAssignment,target.PersonAssignment());
			Assert.AreEqual(1, target.PersonAssignment().PersonalActivities().Count());

		}

		[Test]
		public void VerifyCreateAndAddOvertime()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			target.PersonAssignment().Should().Be.Null();
			target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreEqual(period, target.PersonAssignment().OvertimeActivities().Single().Period);
			target.PersonAssignment().Should().Not.Be.Null();
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWhenAssignmentExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  parameters.Period);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			target.Add(ass);

			target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, target.PersonAssignment());
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWhenAssignmentExistsAndAfterMidnight()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  parameters.Period);
			var nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(ass, mainShift);

			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			target.Add(ass);

			target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, target.PersonAssignment());
		}

		[Test]
		public void ShouldCreateAndAddOvertimeWhenAssignmentIsOvernight()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var assignmentPeriod = new DateTimePeriod(start, end);
			var overtimePeriod = new DateTimePeriod(end, end.AddHours(1));
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(personAssignment.Person, definitionSet);
			target.Add(personAssignment);
			target.CreateAndAddOvertime(activity, overtimePeriod, definitionSet);

			var targetLayer = target.PersonAssignment().OvertimeActivities().Single();
			targetLayer.Payload.Should().Be.SameInstanceAs(activity);
			targetLayer.Period.Should().Be.EqualTo(overtimePeriod);
			targetLayer.DefinitionSet.Should().Be.SameInstanceAs(definitionSet);
		}

		[Test]
		public void VerifyCreateAndAddOvertimeOnSameAssignmentWhenAssignmentIsAdjacent()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var assignmentPeriod = new DateTimePeriod(end, end.AddHours(1));
			var _target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			_target.Add(ass);

			_target.PersonAssignment().Should().Not.Be.Null();
			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, _target.PersonAssignment());
			_target.PersonAssignment().Should().Not.Be.Null();
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWillCreateNewAssignmentWhenAssignmentIsNotAdjacent()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			var assignmentPeriod = new DateTimePeriod(end.AddHours(1), end.AddHours(2));
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			target.Add(ass);

			target.PersonAssignment().Should().Not.Be.Null();
			target.CreateAndAddOvertime(activity, period, definitionSet);
			target.PersonAssignment().OvertimeActivities().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentDoNotExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(period, target.PersonAssignment().PersonalActivities().Single().Period);
		}

		[Test]
		public void VerifyAddMainShiftWithNoZOrder()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test"));
			target.AddMainShift(mainShift);

			mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test1"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test1"));
			target.AddMainShift(mainShift);
			Assert.AreEqual(mainShift.ShiftCategory.Description.Name, target.PersonAssignment().ShiftCategory.Description.Name);
			target.PersonAssignment().Should().Not.Be.Null();
		}

		[Test]
		public void ToStringShouldReturnEmptyString()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			
			Assert.AreEqual("", target.ToString());
		}
		
		private static DateTimePeriod createPeriod(TimeSpan span)
		{
			var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return new DateTimePeriod(date.Add(span), date.Add(span).AddHours(1));
		}
		
		[Test]
		public void VerifyDeleteOvertime()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			
			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(target.Person, definitionSet);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);
			target.CreateAndAddOvertime(activity, period, definitionSet);
 
			Assert.AreEqual(1, target.PersonAssignment().OvertimeActivities().Count());
			target.DeleteOvertime();
			target.PersonAssignment().Should().Not.Be.Null();

			target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreEqual(1, target.PersonAssignment().OvertimeActivities().Count());
			target.DeleteOvertime();
			target.PersonAssignment().Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldNotSplitAbsenceWithSamePeriodAsAssignment()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var start = new DateTime(2011, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2011, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2011, 1, 1);
			var period = new DateTimePeriod(start, end);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person, period);
			var absenceLayer = new AbsenceLayer(new Absence(), period);
			var personAbsence = new PersonAbsence(parameters.Person, parameters.Scenario, absenceLayer);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, dateOnly);
			scheduleDay.Add(personAssignment);
			scheduleDay.Add(personAbsence);

			scheduleDay.DeleteFullDayAbsence(scheduleDay);
			Assert.AreEqual(0, scheduleDay.PersonAbsenceCollection().Count);
		}
		
		[Test]
		public void ShouldDeleteAllAbsencesWithinScheduleDay()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var start1 = new DateTime(2011, 1, 1, 9, 0, 0, DateTimeKind.Utc);
			var end1 = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var start2 = new DateTime(2011, 1, 1, 15, 0, 0, DateTimeKind.Utc);
			var end2 = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var absencePeriod1 = new DateTimePeriod(start1, end1);
			var absencePeriod2 = new DateTimePeriod(start2, end2);
			var dateOnly = new DateOnly(2011, 1, 1);
			var absenceLayer1 = new AbsenceLayer(new Absence(), absencePeriod1);
			var absenceLayer2 = new AbsenceLayer(new Absence(), absencePeriod2);
			var personAbsence1 = new PersonAbsence(parameters.Person, parameters.Scenario, absenceLayer1);
			var personAbsence2 = new PersonAbsence(parameters.Person, parameters.Scenario, absenceLayer2);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, dateOnly);
			scheduleDay.Add(personAbsence1);
			scheduleDay.Add(personAbsence2);

			scheduleDay.DeleteFullDayAbsence(scheduleDay);
			Assert.AreEqual(0, scheduleDay.PersonAbsenceCollection().Count);
		}
		
		[Test]
		public void ShouldReturnNullObjectIfNoScheduleTagExists()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var dateOnly = new DateOnly(2011, 1, 1);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, dateOnly);
			Assert.AreEqual(NullScheduleTag.Instance, scheduleDay.ScheduleTag());
		}
		
		[Test]
		public void ShouldReturnCorrectScheduleTagIfSet()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var dateOnly = new DateOnly(2011, 1, 1);
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, dateOnly);
			var scheduleTag = new ScheduleTag();
			scheduleTag.Description = "hej";
			IAgentDayScheduleTag agentScheduleTag = new AgentDayScheduleTag(parameters.Person, dateOnly,
																			parameters.Scenario, scheduleTag);
			scheduleDay.Clear<IAgentDayScheduleTag>();
			scheduleDay.Add(agentScheduleTag);
			Assert.AreEqual(scheduleTag, scheduleDay.ScheduleTag());

			scheduleTag = new ScheduleTag();
			scheduleTag.Description = "hopp";
			agentScheduleTag = new AgentDayScheduleTag(parameters.Person, dateOnly,
																			parameters.Scenario, scheduleTag);
			scheduleDay.Clear<IAgentDayScheduleTag>();
			scheduleDay.Add(agentScheduleTag);
			Assert.AreEqual(scheduleTag, scheduleDay.ScheduleTag());
		}
		
		[Test]
		public void ShouldAddPersonalActivityToExistingPersonalAssignment()
		{
			var person1 = PersonFactory.CreatePerson();
			var parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), person1, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var scenario = parameters.Scenario;
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod1 = new DateTimePeriod(start, end);
			var dateTimePeriod2 = new DateTimePeriod(end.AddHours(1), end.AddHours(2));

			var target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			target.CreateAndAddPersonalActivity(activity, dateTimePeriod1);
			target.CreateAndAddPersonalActivity(activity, dateTimePeriod2);

			target.PersonAssignment().Should().Not.Be.Null();
			Assert.AreEqual(2, target.PersonAssignment().PersonalActivities().Count());
			Assert.AreEqual(dateTimePeriod1, target.PersonAssignment().PersonalActivities().First().Period);
			Assert.AreEqual(dateTimePeriod2, target.PersonAssignment().PersonalActivities().Last().Period);
		}
	}
}
