using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using System.Linq;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class SchedulePartTest
	{
		private IScheduleDay _target;
		private IScheduleParameters parameters;
		private IScheduleRange range;
		private DateTimePeriod rangePeriod;
		private IPersonAbsence abs;
		private IPersonAssignment ass1;
		private IPersonDayOff pDayOff;
		private IPersonMeeting _personMeeting;
		private IPerson person1;
		private IPerson person2;
		private IScenario scenario;
		private IScheduleDay source;
		private IScheduleDay destination;
		private IScheduleParameters parameters1;
		private IScheduleParameters parameters2;
		private DateTimePeriod period1;
		private DateTimePeriod period2;
		private DateTimePeriod period3;
		private IPersonDayOff personDayOffSource;
		private IPersonAbsence personAbsenceSource;
		private IPersonAbsence personAbsenceSource2;
		private IPersonAssignment personAssignmentSource;
		private IPersonDayOff personDayOffDest;
		private IPersonAbsence personAbsenceDest;
		private IPersonAssignment personAssignmentDest;
		private IScheduleDictionary dic;
		private MockRepository _mocks;
		private TimeZoneInfo timeZoneInfo;
		private IDictionary<IPerson, IScheduleRange> underlyingDictionary;
		private INote _note;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			range = _mocks.StrictMock<IScheduleRange>();
			rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
			timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
			parameters =
				new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), PersonFactory.CreatePerson(), new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			scenario = parameters.Scenario;
			underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			underlyingDictionary.Add(parameters.Person, range);
			_note = new Note(parameters.Person, new DateOnly(2000, 1, 1), scenario, "The agent is very cute");
			
			abs =
				PersonAbsenceFactory.CreatePersonAbsence(parameters.Person, parameters.Scenario,
														 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			_target.Add(abs);

			ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario, parameters.Person,
																	  parameters.Period);

			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description",
				ActivityFactory.CreateActivity("activity"), parameters.Scenario);

			_personMeeting = new PersonMeeting(meeting, new MeetingPerson(parameters.Person, true), rangePeriod);


			_personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(parameters.Person, true));

			_target.Add(_personMeeting);
			_target.Add(ass1);
			_target.Add(_note);

			DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
			dayOff.Anchor = TimeSpan.FromHours(12);
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));
			
			pDayOff = new PersonDayOff(parameters.Person, parameters.Scenario,
										dayOff, new DateOnly(parameters.Period.StartDateTime.Date));

			_target.Add(pDayOff);
		}

		[Test]
		public void ShouldThrowIfTryingToAddMultipleAssignments()
		{
			Assert.Throws<ArgumentException>(() => 
				_target.Add(new PersonAssignment(_target.Person, _target.Scenario, _target.DateOnlyAsPeriod.DateOnly))
			);
		}

		[Test]
		public void VerifyClone()
		{
			IScheduleDay clone = (IScheduleDay)_target.Clone();
			Assert.AreEqual(_target.PersonAbsenceCollection().Count, clone.PersonAbsenceCollection().Count);
			Assert.AreEqual(_target.PersonAssignmentCollection().Count, clone.PersonAssignmentCollection().Count);
			Assert.AreEqual(_target.PersonDayOffCollection().Count, clone.PersonDayOffCollection().Count);
			Assert.AreEqual(_target.PersonMeetingCollection().Count, clone.PersonMeetingCollection().Count);
			Assert.AreEqual(_target.NoteCollection().Count, clone.NoteCollection().Count);
			Assert.IsFalse(clone.FullAccess);
			Assert.IsFalse(clone.IsFullyPublished);
			Assert.AreEqual(_target.OvertimeAvailablityCollection().Count, clone.OvertimeAvailablityCollection().Count);
		}

		[Test]
		public void RemoveEmptyAssignmentsPersonalShift()
		{
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(parameters.Period.StartDateTime));
			var ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(new Activity("sdf"), parameters.Person, parameters.Period, parameters.Scenario);
			_target.Add(ass);
			_target.RemoveEmptyAssignments();
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
			ass.RemoveLayer(ass.PersonalLayers().Single());
			_target.RemoveEmptyAssignments();
			Assert.AreEqual(0, _target.PersonAssignmentCollection().Count);
		}


		[Test]
		public void VerifyProperties()
		{
			Assert.AreEqual(1, _target.PersonAbsenceCollection().Count);
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
			Assert.AreEqual(1, _target.PersonDayOffCollection().Count);
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
			Assert.AreEqual(1, _target.NoteCollection().Count);
			Assert.IsFalse(_target.FullAccess);
			Assert.IsFalse(_target.IsFullyPublished);
			Assert.AreEqual(_target.Person.PermissionInformation.DefaultTimeZone().DisplayName, _target.TimeZone.DisplayName);
			//temp one
			CollectionAssert.IsEmpty(_target.PersonRestrictionCollection());
		}

		[Test]
		public void VerifyCanAdd()
		{
			_target.Add(
				PersonAbsenceFactory.CreatePersonAbsence(parameters.Person, parameters.Scenario,
														 new DateTimePeriod(2000, 12, 1, 2001, 1, 3)));
			Assert.AreEqual(2, _target.PersonAbsenceCollection().Count);
		}

		[Test]
		public void VerifyProjectionsAreSorted()
		{
			var part = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000,1,1));
			var ass = new PersonAssignment(parameters.Person, parameters.Scenario, new DateOnly(2000, 1, 1));
			ass.SetMainShiftLayers(new []
				{
					new MainShiftLayer(new Activity("sdf"), createPeriod(TimeSpan.FromHours(4))), 
					new MainShiftLayer(new Activity("sdf"), createPeriod(TimeSpan.FromHours(1))), 
					new MainShiftLayer(new Activity("sdf"), createPeriod(TimeSpan.FromHours(9)))
				}, new ShiftCategory("sdf"));
			part.Add(ass);

			Assert.AreEqual(new DateTimePeriod(new DateTime(2000, 1, 1, 1, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc)), part.ProjectionService().CreateProjection().Period());
		}

		[Test]
		public void VerifyCreateAndAddDayOff()
		{
			_target.Clear<PersonDayOff>();

			DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(6));
			dayOff.Anchor = TimeSpan.FromHours(12);
			_target.CreateAndAddDayOff(dayOff);
			_target.CreateAndAddDayOff(dayOff);
			Assert.AreEqual(1, _target.PersonDayOffCollection().Count);
			Assert.AreEqual(
				TimeZoneHelper.ConvertToUtc(_target.Period.StartDateTime.Add(dayOff.Anchor),
											_target.Person.PermissionInformation.DefaultTimeZone()),
				_target.PersonDayOffCollection()[0].DayOff.Anchor);
			Assert.AreEqual(_target.Period.StartDateTime, _target.PersonDayOffCollection()[0].DayOff.Anchor.Date);
			Assert.AreEqual(new TimeSpan(24, 0, 0), _target.PersonDayOffCollection()[0].DayOff.TargetLength);
			Assert.AreEqual(TimeSpan.FromHours(6), _target.PersonDayOffCollection()[0].DayOff.Flexibility);
		}


		[Test]
		public void VerifyCreateAndAddAbsence()
		{
			IAbsence absence = AbsenceFactory.CreateAbsence("abs");
			DateTimePeriod period = new DateTimePeriod(2000, 1, 1, 2000, 1, 3);
			AbsenceLayer absLayer = new AbsenceLayer(absence, period);
			_target.Clear<IPersonAbsence>();
			_target.CreateAndAddAbsence(absLayer);
			Assert.AreEqual(1, _target.PersonAbsenceCollection().Count);
			Assert.AreEqual(period, _target.PersonAbsenceCollection()[0].Period);
		}

		[Test]
		public void ShouldCreateAndAddAbsenceWhenAssignmentIsOvernight()
		{
			DateTime start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			DateTimePeriod assignmentPeriod = new DateTimePeriod(start, end);
			DateTimePeriod absencePeriod = new DateTimePeriod(start.AddHours(2), start.AddHours(3));
			IAbsence absence = AbsenceFactory.CreateAbsence("abs");
			AbsenceLayer absLayer = new AbsenceLayer(absence, absencePeriod);
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			_target.Add(personAssignment);
			_target.CreateAndAddAbsence(absLayer);
			Assert.That(absLayer.Id, Is.EqualTo(_target.PersonAbsenceCollection()[0].Id));
		}

		[Test]
		public void VerifyCreateAndAddNote()
		{
			string text = "Green haired agent was sent home";
			Assert.IsTrue(((IList<INote>)_target.NoteCollection()).Count == 1);
			_target.CreateAndAddNote(text);
			Assert.IsTrue(((IList<INote>)_target.NoteCollection()).Count == 1);
			Assert.AreEqual(text, ((IList<INote>)_target.NoteCollection())[0].GetScheduleNote(new NoFormatting()));
		}

		[Test]
		public void VerifyCreateAndAddActivityMainShiftExists()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			var activityLayer = new MainShiftLayer(activity, period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			_target.CreateAndAddActivity(activityLayer, shiftCategory);
			Assert.AreEqual(2, _target.PersonAssignmentCollection()[0].MainLayers().Count());
			Assert.AreEqual(activityLayer.Period, _target.PersonAssignmentCollection()[0].MainLayers().Last().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityMainShiftOverMidnightExists()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			var activityLayer = new MainShiftLayer(activity, period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			DateTimePeriod nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));

			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(_target.PersonAssignmentCollection()[0], mainShift);

			_target.CreateAndAddActivity(activityLayer, shiftCategory);
			Assert.AreEqual(2, _target.PersonAssignmentCollection()[0].MainLayers().Count());
			Assert.AreEqual(activityLayer.Period, _target.PersonAssignmentCollection()[0].MainLayers().Last().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityNoAssignment()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			var activityLayer = new MainShiftLayer(activity, period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			_target.CreateAndAddActivity(activityLayer, shiftCategory);
			Assert.AreEqual(activityLayer.Period, _target.PersonAssignmentCollection()[0].MainLayers().Single().Period);
		}

		[Test]
		public void VerifyCreateAndAddActivityNoMainShift()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			var activityLayer = new MainShiftLayer(activity, period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment assNoMainShift = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(activity, parameters.Person,
																	  parameters.Period, parameters.Scenario);
			_target.Add(assNoMainShift);

			_target.CreateAndAddActivity(activityLayer, shiftCategory);
			Assert.AreEqual(activityLayer.Period, _target.PersonAssignmentCollection()[0].MainLayers().Single().Period);
		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentExists()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			_target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(1, _target.PersonAssignmentCollection()[0].PersonalLayers().Count());
			Assert.AreEqual(period, _target.PersonAssignmentCollection()[0].PersonalLayers().First().Period);

		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentExistsAndAfterMidnight()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			DateTimePeriod nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(_target.PersonAssignmentCollection()[0], mainShift);

			_target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(period, _target.PersonAssignmentCollection()[0].PersonalLayers().Single().Period);
		}

        [Test]
		public void ShouldNotCreateNewAssignmentWhenAdjacentToAssignment()
		{
			IShiftCategory shiftCategory = new ShiftCategory("ShiftCategory");
			
			IActivity activity = ActivityFactory.CreateActivity("activity");
			IActivity activity2 = ActivityFactory.CreateActivity("activity2");
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));

			var startAssignment = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            var endAssignment = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
            var periodAssignment = new DateTimePeriod(startAssignment, endAssignment);

			var startPersonalShift = new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var endPersonalShift = new DateTime(2000, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			var periodPersonalShift = new DateTimePeriod(startPersonalShift, endPersonalShift);

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, _target.Person, periodAssignment, shiftCategory, parameters.Scenario);
			_target.Add(personAssignment);
	        _target.CreateAndAddPersonalActivity(activity2, periodPersonalShift);

			Assert.AreEqual(1,_target.PersonAssignmentCollection().Count);
			Assert.AreEqual(personAssignment,_target.PersonAssignmentCollection()[0]);
			Assert.AreEqual(1, _target.PersonAssignmentCollection()[0].PersonalLayers().Count());

		}

		[Test]
		public void VerifyCreateAndAddOvertime()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(_target.Person, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			Assert.AreEqual(0, _target.PersonAssignmentCollection().Count);
			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreEqual(period, _target.PersonAssignmentCollection()[0].OvertimeLayers().Single().Period);
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWhenAssignmentExists()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  parameters.Period);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			_target.Add(ass);

			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, _target.PersonAssignmentCollection()[0]);
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWhenAssignmentExistsAndAfterMidnight()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 2, 2, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 2, 3, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  parameters.Period);
			DateTimePeriod nightPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc));
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			var mainShift = EditableShiftFactory.CreateEditorShift(new Activity("hej"), nightPeriod, shiftCategory);
			new EditableShiftMapper().SetMainShiftLayers(ass, mainShift);

			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			_target.Add(ass);

			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, _target.PersonAssignmentCollection()[0]);
		}

		[Test]
		public void ShouldCreateAndAddOvertimeWhenAssignmentIsOvernight()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 23, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			DateTimePeriod assignmentPeriod = new DateTimePeriod(start, end);
			DateTimePeriod overtimePeriod = new DateTimePeriod(end, end.AddHours(1));
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(personAssignment.Person, definitionSet);
			_target.Add(personAssignment);
			_target.CreateAndAddOvertime(activity, overtimePeriod, definitionSet);

			var targetLayer = _target.PersonAssignmentCollection().Single().OvertimeLayers().Single();
			targetLayer.Payload.Should().Be.SameInstanceAs(activity);
			targetLayer.Period.Should().Be.EqualTo(overtimePeriod);
			targetLayer.DefinitionSet.Should().Be.SameInstanceAs(definitionSet);
		}

		[Test]
		public void VerifyCreateAndAddOvertimeOnSameAssignmentWhenAssignmentIsAdjacent()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			DateTimePeriod assignmentPeriod = new DateTimePeriod(end, end.AddHours(1));
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			_target.Add(ass);

			Assert.IsTrue(1 == _target.PersonAssignmentCollection().Count);
			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreSame(ass, _target.PersonAssignmentCollection()[0]);
			Assert.IsTrue(1 == _target.PersonAssignmentCollection().Count);
		}

		[Test]
		public void VerifyCreateAndAddOvertimeWillCreateNewAssignmentWhenAssignmentIsNotAdjacent()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime",
																					   MultiplicatorType.Overtime);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			DateTimePeriod assignmentPeriod = new DateTimePeriod(end.AddHours(1), end.AddHours(2));
			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(parameters.Scenario,
																						  parameters.Person,
																						  assignmentPeriod);
			PersonFactory.AddDefinitionSetToPerson(ass.Person, definitionSet);
			_target.Add(ass);

			Assert.IsTrue(1 == _target.PersonAssignmentCollection().Count);
			_target.CreateAndAddOvertime(activity, period, definitionSet);
			_target.AssignmentHighZOrder().OvertimeLayers().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void VerifyCreateAndAddPersonalActivityPersonalAssignmentDoNotExists()
		{
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			_target.Clear<IPersonAssignment>();

			_target.CreateAndAddPersonalActivity(activity, period);
			Assert.AreEqual(period, _target.PersonAssignmentCollection()[0].PersonalLayers().Single().Period);
		}

		[Test]
		public void VerifyAddMainShiftWithNoZOrder()
		{
			_target.Clear<IPersistableScheduleData>();

			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test"));
			_target.AddMainShift(mainShift);

			mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test1"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test1"));
			_target.AddMainShift(mainShift);
			Assert.AreEqual(mainShift.ShiftCategory.Description.Name, _target.AssignmentHighZOrder().ShiftCategory.Description.Name);
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
		}

		[Test]
		public void TwoPersonAssignmentsOnOneDayShouldThrow()
		{
			_target.Clear<IPersistableScheduleData>();

			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			var mainShift = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test"));
			_target.AddMainShift(mainShift);

			start = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc);
			end = new DateTime(2000, 1, 1, 14, 0, 0, DateTimeKind.Utc);
			period = new DateTimePeriod(start, end);

			var mainShift1 = EditableShiftFactory.CreateEditorShift(ActivityFactory.CreateActivity("test1"), period,
																	ShiftCategoryFactory.CreateShiftCategory("test1"));
			IPersonAssignment personAssignment = PersonAssignmentFactory.CreatePersonAssignment(_target.Person, _target.Scenario);
			new EditableShiftMapper().SetMainShiftLayers(personAssignment, mainShift1);

			Assert.Throws<ArgumentException>(() =>
			                                         _target.Add(personAssignment));
		}

		[Test]
		public void ToStringShouldReturnEmptyString()
		{
			Assert.AreEqual("", _target.ToString());

		}

		#region Merge

		[Test]
		public void VerifyMergeEmptyDay()
		{
			SetupForMergeTests();
			//add dayoff, absence, assignment to destination
			destination.Add(personDayOffDest);
			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			//merge on from an empty source
			destination.Merge(source, false);

			//check that destination is not affected
			Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
			Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
		}

		[Test]
		public void VerifyMergeDayOff()
		{
			SetupForMergeTests();
			//add dayoff to source
			source.Add(personDayOffSource);
			//source.Add(personAbsenceSource);
			//source.Add(personAssignmentSource);

			//add dayoff, absence, assignment to destination
			destination.Add(personDayOffDest);
			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			//verify nothing is changed when no permission
		    var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
			using(_mocks.Record())
			{
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(false);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    destination.Merge(source, false);
                    Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
                    Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
                    Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);

                    //verify that day off is pasted, absencense is splitted and assignment is removed
                    destination.Merge(source, false);
                    Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
                    Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
                    Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);
                }
            }
		}

		[Test]
		public void VerifyMergeDayOffNoPermissionModifyAssignment()
		{
			SetupForMergeTests();
			source.Add(personDayOffSource);
			destination.Add(personAssignmentDest);

		    var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
			using (_mocks.Record())
			{
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(false);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    destination.Merge(source, false);
                    Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
                }
            }
		}

		[Test]
		public void VerifyMergeDayOffNoPermissionModifyAbsence()
		{
			SetupForMergeTests();
			source.Add(personDayOffSource);
			destination.Add(personAbsenceDest);

		    var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
			using (_mocks.Record())
			{
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(false);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    destination.Merge(source, false);
                    Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
                }
            }
		}

		[Test]
		public void ShouldNotClearAbsenceDayAfterAddingDayOff()
		{
			SetupForMergeTests();

			source.Add(personDayOffSource);

			DateTimePeriod periodDayAfter = new DateTimePeriod(new DateTime(2000, 1, 4, 8, 0, 0, DateTimeKind.Utc),
			   new DateTime(2000, 1, 4, 17, 0, 0, DateTimeKind.Utc));

			IPersonAbsence personAbsenceDayAfter = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), periodDayAfter));

			destination.Add(personAbsenceDayAfter);

			destination.Merge(source, false);
			Assert.IsTrue(destination.PersistableScheduleDataCollection().Contains(personAbsenceDayAfter));
		}

		[Test]
		public void VerifyCreateAndAddActivityNoPermissionModifyDayOff()
		{
			SetupForMergeTests();
			destination.Add(personDayOffDest);
			var activityLayer = new MainShiftLayer(ActivityFactory.CreateActivity("activity"), destination.Period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

		    var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
			using (_mocks.Record())
			{
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(false);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    destination.CreateAndAddActivity(activityLayer, shiftCategory);
                    Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
                }
            }
		}

		[Test]
		public void VerifyCreateAndAddActivityOnFullDayAbsence()
		{
			SetupForMergeTests();
			destination.Add(personAbsenceDest);

			var activityLayer = new MainShiftLayer(ActivityFactory.CreateActivity("activity"), destination.Period);
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			destination.CreateAndAddActivity(activityLayer, shiftCategory);
			Assert.AreEqual(0, destination.PersonAbsenceCollection().Count);
		}

		[Test]
		public void VerifyMergePreference()
		{
			SetupForMergeTests();

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceRestriction preferenceRestriction2 = new PreferenceRestriction();
			preferenceRestriction.SetId(new Guid());
			preferenceRestriction2.SetId(new Guid());
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), preferenceRestriction);
			IPreferenceDay preferenceDay2 = new PreferenceDay(destination.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), preferenceRestriction2);


			source.Add(preferenceDay);
			destination.Add(preferenceDay2);

			destination.Merge(source, false);

			Assert.AreEqual(1,((IList<IRestrictionBase>)destination.RestrictionCollection()).Count);
			Assert.IsTrue(((IList<IRestrictionBase>)destination.RestrictionCollection())[0] is PreferenceRestriction);
			
			foreach (IPreferenceDay prefDay in destination.PersistableScheduleDataCollection())
			{
				Assert.AreEqual(destination.Period.StartDateTimeLocal(timeZoneInfo).Date, prefDay.RestrictionDate.Date);
				Assert.IsNull(prefDay.Restriction.Id);
			}
		}

		[Test]
		public void VerifyDeletePreference()
		{
			SetupForMergeTests();

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), preferenceRestriction);

			source.Add(preferenceDay);

			source.DeletePreferenceRestriction();

			Assert.IsTrue(((IList<IRestrictionBase>)source.RestrictionCollection()).Count == 0);
		}

		[Test]
		public void VerifyDeleteNote()
		{
			SetupForMergeTests();

			INote note = new Note(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), source.Scenario, "Oh my God");
			source.Add(note);
			Assert.IsTrue(((IList<INote>)source.NoteCollection()).Count == 1);
			source.DeleteNote();

			Assert.IsTrue(((IList<INote>)source.NoteCollection()).Count == 0);
		}

		[Test]
		public void VerifyMergeStudentAvailability()
		{
			SetupForMergeTests();

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), list);

			source.Add(studentAvailabilityDay);

			destination.Merge(source, false);

			Assert.IsTrue(((IList<IRestrictionBase>)destination.RestrictionCollection())[0] is StudentAvailabilityRestriction);

			foreach (IStudentAvailabilityDay studDay in destination.PersistableScheduleDataCollection())
			{
				Assert.AreEqual(destination.Period.StartDateTimeLocal(timeZoneInfo).Date, studDay.RestrictionDate.Date);
			}
		}

		[Test]
		public void VerifyDeleteStudentAvailability()
		{
			SetupForMergeTests();

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), list);

			source.Add(studentAvailabilityDay);

			source.DeleteStudentAvailabilityRestriction();

			Assert.IsTrue(((IList<IRestrictionBase>)source.RestrictionCollection()).Count == 0);
		}

		[Test]
		public void VerifyDeleteOvertimeAvailability()
		{
			SetupForMergeTests();

			var overtimeAvailabilityDay = new OvertimeAvailability(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), TimeSpan.FromHours(17), TimeSpan.FromHours(19));

			source.Add(overtimeAvailabilityDay);

			source.DeleteOvertimeAvailability();

			Assert.IsTrue(source.OvertimeAvailablityCollection().Count == 0);
		}

		[Test]
		public void VerifyDeleteAbsence()
		{
			SetupForMergeTests();

			DateTime absStart1 = new DateTime(2000,1,1,10,0,0,DateTimeKind.Utc);
			DateTime absEnd1 = new DateTime(2000,1,1,12,0,0,DateTimeKind.Utc);
			DateTime absStart2 = new DateTime(2000,1,1,13,0,0, DateTimeKind.Utc);
			DateTime absEnd2 = new DateTime(2000,1,1,14,0,0,DateTimeKind.Utc);

			DateTimePeriod absPeriod1 = new DateTimePeriod(absStart1,absEnd1);
			DateTimePeriod absPeriod2 = new DateTimePeriod(absStart2, absEnd2);
			DateTimePeriod assPeriod = new DateTimePeriod(absStart1,absEnd2);
												  
			IAbsenceLayer absenceLayer1 = new AbsenceLayer(new Absence(), absPeriod1);
			IAbsenceLayer absenceLayer2 = new AbsenceLayer(new Absence(), absPeriod2);

			IPersonAbsence personAbsence1 = new PersonAbsence(person1, scenario, absenceLayer1);
			IPersonAbsence personAbsence2 = new PersonAbsence(person1, scenario, absenceLayer2);
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person1, assPeriod);

			//without assignment
			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 2);
			source.DeleteAbsence(true); //special delete
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 0);

			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 2);
			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 1);

			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 0);

			//with assignment
			source.Add(ass);
			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 2);
			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Count == 1);
		}

		[Test]
		public void ShouldNotDeleteAbsenceOnNextDay()
		{
			SetupForMergeTests();

			DateTime absStart1 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime absEnd1 = new DateTime(2000, 1, 1, 23, 59, 0, DateTimeKind.Utc);
			DateTime absStart2 = new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			DateTime absEnd2 = new DateTime(2000, 1, 2, 23, 59, 0, DateTimeKind.Utc);

			DateTimePeriod absPeriod1 = new DateTimePeriod(absStart1, absEnd1);
			DateTimePeriod absPeriod2 = new DateTimePeriod(absStart2, absEnd2);

			IAbsenceLayer absenceLayer1 = new AbsenceLayer(new Absence(), absPeriod1);
			IAbsenceLayer absenceLayer2 = new AbsenceLayer(new Absence(), absPeriod2);

			IPersonAbsence personAbsence1 = new PersonAbsence(person1, scenario, absenceLayer1);
			IPersonAbsence personAbsence2 = new PersonAbsence(person1, scenario, absenceLayer2);

			source.Add(personAbsence1);
			source.Add(personAbsence2);
			
			Assert.AreEqual(2, source.PersistableScheduleDataCollection().Count());
			source.DeleteAbsence(true);
			Assert.AreEqual(1, source.PersistableScheduleDataCollection().Count());
		}

		[Test]
		public void ShouldDeleteLastAddedAbsence()
		{
			SetupForMergeTests();

			DateTime absStart1 = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime absEnd1 = new DateTime(2000, 1, 1, 23, 59, 0, DateTimeKind.Utc);
			DateTime absStart2 = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			DateTime absEnd2 = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);

			DateTimePeriod absPeriod1 = new DateTimePeriod(absStart1, absEnd1);
			DateTimePeriod absPeriod2 = new DateTimePeriod(absStart2, absEnd2);

			IAbsenceLayer absenceLayer1 = new AbsenceLayer(new Absence(), absPeriod1);
			IAbsenceLayer absenceLayer2 = new AbsenceLayer(new Absence(), absPeriod2);

			IPersonAbsence personAbsence1 = new PersonAbsence(person1, scenario, absenceLayer1);
			IPersonAbsence personAbsence2 = new PersonAbsence(person1, scenario, absenceLayer2);

			source.Add(personAbsence1);
			source.Add(personAbsence2);

			Assert.AreEqual(2, source.PersistableScheduleDataCollection().Count());
			source.DeleteAbsence(false);
			Assert.AreEqual(1, source.PersistableScheduleDataCollection().Count());
		    var a = source.PersonAbsenceCollection()[0];
			Assert.AreEqual(personAbsence1, a);
		}

		[Test]
		public void VerifyMergeFullDayAbsence()
		{
			SetupForMergeTests();

			using (_mocks.Record())
			{
			}

			using (_mocks.Playback())
			{
				//add absences, assignment to source
				source.Add(personAbsenceSource);
				source.Add(personAbsenceSource2);
				source.Add(personAssignmentSource);

				//add dayoff, absence, assignment to destination
				destination.Add(personDayOffDest);
				destination.Add(personAbsenceDest);
				destination.Add(personAssignmentDest);

				//merge
				((ExtractedSchedule)destination).MergeAbsences(source, true);

				//assert that absences is pasted and nothing else changes
				Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
				Assert.AreEqual(3, destination.PersonAbsenceCollection().Count);
				Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
			}
		}

        [Test]
        public void ShouldPasteAbsenceFromSourceWithContractDayOff()
        {
            SetupForMergeTests();

            var absenceService = _mocks.StrictMock<ISignificantPartService>(); //Service for easier testing with Significantpart

            using (_mocks.Record())
            {
                Expect.Call(absenceService.SignificantPart()).Return(SchedulePartView.Absence).Repeat.Any();
            }

            //add absences, assignment to source
            source.Add(personAbsenceSource);
            source.Add(personAbsenceSource2);
            //source.Add(personAssignmentSource);

            //add dayoff, absence, assignment to destination
            //destination.Add(personDayOffDest);
            destination.Add(personAbsenceDest);
            //destination.Add(personAssignmentDest);

            //merge
            ((ExtractedSchedule)source).ServiceForSignificantPart = absenceService; //Setup for returning Absence;

            Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
            destination.Merge(source, false);
            Assert.AreEqual(2, destination.PersonAbsenceCollection().Count);
        }

		[Test]
		public void VerifyMergeAbsence()
		{
			SetupForMergeTests();
			DateTimePeriod period = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			IPersonAbsence personAbsenceNotFullDay = PersonAbsenceFactory.CreatePersonAbsence(person1, scenario, period);

			ISignificantPartService fullDayAbsenceService = _mocks.StrictMock<ISignificantPartService>();
			using(_mocks.Record())
			{
				Expect.Call(fullDayAbsenceService.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
			}

			//add absence to source
			source.Add(personAbsenceNotFullDay);

			//add dayoff, absence, assignment to destination
			destination.Add(personDayOffDest);
			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			((ExtractedSchedule)source).ServiceForSignificantPart = fullDayAbsenceService; //Setup full day absence
			
			destination.Merge(source, false);

			//assert the absence is pasted and nothing else changes
			Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(2, destination.PersonAbsenceCollection().Count);
			Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);

		}

		[Test]
		public void VerifyMergeAbsenceDaylightSaving()
		{
			IPerson person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			DateOnly date = new DateOnly(2010,3,22);
			//source
			DateTime dateTimeSourceStart = new DateTime(2010,3,22,8,0,0,DateTimeKind.Utc);

			//absence source
			DateTime dateTimeAbsenceStart = dateTimeSourceStart.AddHours(2);
			DateTime dateTimeAbsenceEnd = dateTimeAbsenceStart.AddHours(2);

			DateTimePeriod periodAbsence = new DateTimePeriod(dateTimeAbsenceStart, dateTimeAbsenceEnd);

			source = ExtractedSchedule.CreateScheduleDay(dic, person,date);
			destination = ExtractedSchedule.CreateScheduleDay(dic, person, date.AddDays(10));
			
			ISignificantPartService absenceService = _mocks.StrictMock<ISignificantPartService>();
			((ExtractedSchedule)source).ServiceForSignificantPart = absenceService;

			using (_mocks.Record())
			{
				Expect.Call(absenceService.SignificantPart()).Return(SchedulePartView.Absence).Repeat.Any();
			}

			IPersonAbsence absence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, periodAbsence);
			source.Add(absence);
		 
			destination.Merge(source, false);
			Assert.AreEqual(periodAbsence.LocalStartDateTime.TimeOfDay, destination.PersonAbsenceCollection()[0].Period.LocalStartDateTime.TimeOfDay);
		}

		[Test] //test function that calculates diff between source and destination, used in copy/paste
		public void VerifyCalculateDiff()
		{
			TimeSpan diff;
			TimeSpan expectedDiff;


			IPerson person = PersonFactory.CreatePerson();

			//paste from wintertime to wintertime("W. Europe Standard Time")
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"))); 
			expectedDiff = new TimeSpan(2, 0, 0, 0);
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 23));
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 25));
			diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);


			//paste from wintertime to day of change to summertime("W. Europe Standard Time")
			expectedDiff = new TimeSpan(5, 0, 0, 0);
			testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 23));
			testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 28));
			diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);

			//paste from wintertime to summertime("W. Europe Standard Time")
			expectedDiff = new TimeSpan(6, 23, 0, 0);
			testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 23));
			testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 30));
			diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);

			//paste from summertime to wintertime("W. Europe Standard Time")
			expectedDiff = new TimeSpan(-6, -23, 0, 0);
			testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 30));
			testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2010, 3, 23));
			diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);

		}

		[Test]
		public void VerifyMergeMainShift()
		{
			SetupForMergeTests();

			source.Add(personAssignmentSource);

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			//verify nothing changes with no permissions

		    var authorization = _mocks.StrictMock<IPrincipalAuthorization>();
			using (_mocks.Record())
			{
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(false);
				Expect.Call(authorization.IsPermitted("")).IgnoreArguments().Return(true).Repeat.Times(3);
			}
            using (_mocks.Playback())
            {
                using (new CustomAuthorizationContext(authorization))
                {
                    destination.Merge(source, false);
                    Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
                    Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);

                    //with permissions
                    destination.Add(personDayOffDest);
                    destination.Merge(source, false);
                    //assert dayoff is still there
                    Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
                    //assert absence is splitted
                    Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
                    //assert we still have 1 assignment
                    Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
                    //assert destination have got the source period(Time)
                    Assert.AreEqual(
                        source.PersonAssignmentCollection()[0].MainLayers().First().Period.TimePeriod(
                            TimeZoneHelper.CurrentSessionTimeZone),
												destination.PersonAssignmentCollection()[0].MainLayers().First().Period.TimePeriod(
                            TimeZoneHelper.CurrentSessionTimeZone));

                    //clear assignments in destination
                    destination.Clear<IPersonAssignment>();
                    //merge
                    destination.Merge(source, false);
                    //assert a new assignment is created in destination
                    Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
                    //assert dayoff is removed
                    Assert.AreEqual(0, destination.PersonDayOffCollection().Count);
                }
            }
		}

		[Test]
		public void VerifyMergePersonalStuff()
		{
			SetupForMergeTests();
			//add dayoff, absence, assignment to destination
			destination.Add(personDayOffDest);
			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			//create personassignment with no mainshift
			IPersonAssignment newPersonAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);
			//add personal layer to assignment
			newPersonAssignment.AddPersonalLayer(ActivityFactory.CreateActivity("activity"), period3);
			//add assignment to source
			source.Add(newPersonAssignment);

			using (_mocks.Record())
			{
			}
			using (_mocks.Playback())
			{
				//merge
				destination.Merge(source, false);

				//assert we still have 1 assignment in destination
				Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
				//assert the personal shift was added
				Assert.AreEqual(2, destination.PersonAssignmentCollection()[0].PersonalLayers().Count());

				//clear assignments in destination
				destination.Clear<IPersonAssignment>();
				//merge
				destination.Merge(source, false);

				//assert the personal shift was added and a new assignment was added
				Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);
			}
		}

	   
		[Test]
		public void VerifyMergeDelete()
		{
			SetupForMergeTests();

			ISignificantPartService absenceService = _mocks.StrictMock<ISignificantPartService>(); //Service for easier testing with Significantpart
			using (_mocks.Record())
			{
				Expect.Call(absenceService.SignificantPart()).Return(SchedulePartView.ContractDayOff).Repeat.Any();
			}

			//add dayoff, absence, assignment to source
			destination.Add(personDayOffDest);
			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			//merge
			destination.Merge(destination, true);

			//assert the absence is splitted
			Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
			Assert.AreEqual(1, destination.PersonAssignmentCollection().Count);

			//merge
			destination.Merge(destination, true);

			//assert the main shift is removed
			Assert.AreEqual(1, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
			Assert.IsNull(destination.PersonAssignmentCollection()[0].ShiftCategory);

			//merge
			destination.Merge(destination, true);

			//assert the day off is removed
			Assert.AreEqual(0, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
			Assert.IsNull(destination.PersonAssignmentCollection()[0].ShiftCategory);

			//merge
			destination.Merge(destination, true);

			//assert the assignment is removed
			Assert.AreEqual(0, destination.PersonDayOffCollection().Count);
			Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
			Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);

			//create assignment with no mainshift
			IPersonAssignment newPersonAssignment = PersonAssignmentFactory.CreatePersonAssignment(person2, scenario);
			//add a personal layer
			newPersonAssignment.AddPersonalLayer(ActivityFactory.CreateActivity("activity"), period1.MovePeriod(TimeSpan.FromDays(1)));

			//add assignment to destination
			destination.Add(newPersonAssignment);

			//merge
			destination.Merge(destination, true);

			//assert personal shift + assignment is removed
			Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);

			var period = new DateTimePeriod(new DateTime(2000, 1, 2, 8, 0, 0, DateTimeKind.Utc),
			   new DateTime(2000, 1, 2, 17, 0, 0, DateTimeKind.Utc));

			//create absence
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person2, scenario, period);
			//add personal layer to assignment with no mainshift
			newPersonAssignment.AddPersonalLayer(ActivityFactory.CreateActivity("activity"), period1.MovePeriod(TimeSpan.FromDays(1)));

			//clear absences
			destination.Clear<IPersonAbsence>();
			//add absence
			destination.Add(personAbsence);
			//add assignment
			destination.Add(newPersonAssignment);
			//merge
			destination.Merge(destination, true);

			//assert personal shift is removed
			Assert.AreEqual(1, destination.PersonAbsenceCollection(true).Count);
			Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);


			((ExtractedSchedule)destination).ServiceForSignificantPart = absenceService; //Setup for returning Absence;

			//assert absence is removed
			destination.Merge(destination, true);
			Assert.AreEqual(0, destination.PersonAbsenceCollection(true).Count);

			//create absence
			var personAbsence2 = PersonAbsenceFactory.CreatePersonAbsence(person2, scenario, destination.Period);
			destination.Add(personAbsence2);
			destination.Merge(destination, true);
			Assert.AreEqual(0, destination.PersonAbsenceCollection(true).Count);
		}


		private void SetupForMergeTests()
		{
			//person 1
			person1 = PersonFactory.CreatePerson();
			//person 2
			person2 = PersonFactory.CreatePerson();

			//some periods to use
			period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc),
			   new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc));

			period2 = new DateTimePeriod(new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			   new DateTime(2001, 10, 1, 17, 0, 0, DateTimeKind.Utc));

			period3 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc),
			   new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc));

			//some parameters to use
			parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

				//the destination in a merge
			destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			//absence, dayoff, assignment for source
			personAbsenceSource = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), parameters1.Period));
			personAbsenceSource2 = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs2"), parameters1.Period));

			DayOffTemplate dayOff = new DayOffTemplate(new Description("test"));
			dayOff.Anchor = TimeSpan.FromHours(3);
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(35), TimeSpan.FromHours(1));
			personDayOffSource = new PersonDayOff(person1, scenario, dayOff, new DateOnly(period1.StartDateTime.Date));

			personAssignmentSource = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
										ActivityFactory.CreateActivity("sdfsdf"),
										person1,
										period1,
										ShiftCategoryFactory.CreateShiftCategory("shiftCategory"),
										scenario);

			//absence, dayoff, assignment for destination
			personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			DayOffTemplate dayOffDest = new DayOffTemplate(new Description("test"));
			dayOffDest.Anchor = TimeSpan.FromHours(3);
			dayOff.SetTargetAndFlexibility(TimeSpan.FromHours(35), TimeSpan.FromHours(1));
			personDayOffDest = new PersonDayOff(person2, scenario, dayOffDest, new DateOnly(period1.StartDateTime.Date.AddDays(1)));

			personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
										ActivityFactory.CreateActivity("sdfsdf"),
										person2,
										period3.MovePeriod(TimeSpan.FromDays(1)),
										ShiftCategoryFactory.CreateShiftCategory("shiftCategory"),
										scenario);
		}

        [Test]
        public void VerifyCopyPasteToAnotherTimeZoneChangesDateOnlyAsPeriod()
        {
            IPerson sourcePerson = PersonFactory.CreatePerson();
            sourcePerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

            IPerson targetPersonInSameTimezone = PersonFactory.CreatePerson();
            targetPersonInSameTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

            IPerson targetPersonInDifferentTimezone = PersonFactory.CreatePerson();
            targetPersonInDifferentTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());

            DateOnly day = new DateOnly(2011, 02, 10);
            DateTime start = new DateTime(2011, 2, 10, 16, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2011, 2, 10, 20, 0, 0, 0, DateTimeKind.Utc);

            IScheduleDay sourceDay = ExtractedSchedule.CreateScheduleDay(dic, sourcePerson, day);
            IActivity activity = ActivityFactory.CreateActivity("Test");

            DateTimePeriod period = new DateTimePeriod(start, end);
            ShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Test");
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, shiftCategory);
            sourceDay.AddMainShift(mainShift);

            IScheduleDay targetDaySameTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInSameTimezone, day);
            IScheduleDay targetDayDifferentTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInDifferentTimezone, day);

            targetDaySameTimezone.Merge(sourceDay, false, true);

            targetDayDifferentTimezone.Merge(sourceDay, false, true);

            DateOnly expectedDateOnly = day;
            Assert.AreEqual(expectedDateOnly, targetDaySameTimezone.DateOnlyAsPeriod.DateOnly);

            expectedDateOnly = day.AddDays(1);
            Assert.AreEqual(expectedDateOnly, targetDayDifferentTimezone.DateOnlyAsPeriod.DateOnly);
        }

        [Test]
        public void VerifyMergingWithAnotherTimeZoneNotChangesDateOnlyAsPeriod()
        {
            IPerson sourcePerson = PersonFactory.CreatePerson();
            sourcePerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

            IPerson targetPersonInSameTimezone = PersonFactory.CreatePerson();
            targetPersonInSameTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

            IPerson targetPersonInDifferentTimezone = PersonFactory.CreatePerson();
            targetPersonInDifferentTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());

            DateOnly day = new DateOnly(2011, 02, 10);
            DateTime start = new DateTime(2011, 2, 10, 16, 0, 0, 0, DateTimeKind.Utc);
            DateTime end = new DateTime(2011, 2, 10, 20, 0, 0, 0, DateTimeKind.Utc);

            IScheduleDay sourceDay = ExtractedSchedule.CreateScheduleDay(dic, sourcePerson, day);
            IActivity activity = ActivityFactory.CreateActivity("Test");

            DateTimePeriod period = new DateTimePeriod(start, end);
            ShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Test");
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, shiftCategory);
            sourceDay.AddMainShift(mainShift);

            IScheduleDay targetDaySameTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInSameTimezone, day);
            IScheduleDay targetDayDifferentTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInDifferentTimezone, day);

            targetDaySameTimezone.Merge(sourceDay, false, false);

            targetDayDifferentTimezone.Merge(sourceDay, false, false);

            DateOnly expectedDateOnly = day;
            Assert.AreEqual(expectedDateOnly, targetDaySameTimezone.DateOnlyAsPeriod.DateOnly);

            expectedDateOnly = day;
            Assert.AreEqual(expectedDateOnly, targetDayDifferentTimezone.DateOnlyAsPeriod.DateOnly);
        }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		private void SetupForRemoveOptionsTest()
		{
			SetupForMergeTests();

			source.Add(personDayOffSource);
			source.Add(personAbsenceSource);
			source.Add(personAssignmentSource);

			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			source.CreateAndAddOvertime(activity, period, definitionSet);

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.SetId(new Guid());
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), preferenceRestriction);

			source.Add(preferenceDay);

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfo)), list);

			source.Add(studentAvailabilityDay);
		}

		#endregion

		#region Tests for SignificantPart
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void SignificantPartWithMainShiftAndDayOff()
		{
			IPerson person = PersonFactory.CreatePerson();
			DateTimePeriod period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
			IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2001,1,1));

			IPersonDayOff personDayOff = PersonDayOffFactory.CreatePersonDayOff(person, scenario,
																			   TimeZoneHelper.ConvertToUtc(
																				   period.StartDateTime,
																				   person.PermissionInformation.
																					   DefaultTimeZone()).Date,
																			   TimeSpan.FromDays(1),
																			   TimeSpan.FromHours(1),
																			   TimeSpan.FromHours(12));
			part.Add(personDayOff);

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceDay preferenceDay = new PreferenceDay(part.Person, new DateOnly(part.Period.StartDateTimeLocal(timeZoneInfo)), preferenceRestriction);

			part.Add(preferenceDay);

			Assert.AreEqual(SchedulePartView.DayOff, part.SignificantPart());

			part.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, period));
			Assert.AreEqual(SchedulePartView.MainShift, part.SignificantPart());

			part.Add(PersonAbsenceFactory.CreatePersonAbsence(person, scenario, period));
			Assert.AreEqual(SchedulePartView.DayOff, part.SignificantPart());

			part.Clear<IPersonAbsence>();
			Assert.AreEqual(SchedulePartView.MainShift, part.SignificantPart());

			part.Clear<IPersonAssignment>();
			part.Clear<IPersonAbsence>();
			part.Clear<IPersonDayOff>();

			Assert.AreEqual(SchedulePartView.PreferenceRestriction, part.SignificantPart());
		}

		[Test]
		public void SignificantPartWithoutMainShiftWithPersonalShift()
		{
			IPerson person = PersonFactory.CreatePerson();
			DateTimePeriod period = new DateTimePeriod(2001, 1, 1, 2001, 1, 2);
			IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2001, 1, 1));

			part.Add(PersonAssignmentFactory.CreateAssignmentWithPersonalShift(ActivityFactory.CreateActivity("d"), person, period, scenario));

			Assert.AreEqual(SchedulePartView.PersonalShift, part.SignificantPart());

			part.Add(PersonDayOffFactory.CreatePersonDayOff(person, scenario, TimeZoneHelper.ConvertToUtc(period.StartDateTime, person.PermissionInformation.DefaultTimeZone()).Date, TimeSpan.FromDays(1), TimeSpan.FromHours(1), new TimeSpan()));
			Assert.AreEqual(SchedulePartView.DayOff, part.SignificantPart());
			Assert.AreEqual(SchedulePartView.DayOff, part.SignificantPart());
		}

		[Test]
		public void SignificantPartCallsService()
		{
			IPerson person = PersonFactory.CreatePerson();
			IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2001,1,1));

			Assert.AreEqual(part.SignificantPart(), SchedulePartView.None);

			ISignificantPartService service = _mocks.StrictMock<ISignificantPartService>();
			((ExtractedSchedule)part).ServiceForSignificantPart = service;

			using(_mocks.Record())
			{
				Expect.Call(service.SignificantPart()).Return(SchedulePartView.MainShift);
			}
			using(_mocks.Playback())
			{
				Assert.AreEqual(SchedulePartView.MainShift,part.SignificantPart());
			}
		}

		[Test]
		public void SignificantPartEmptyShouldReturnNone()
		{

			IScheduleDay part = ExtractedSchedule.CreateScheduleDay(dic,
														   parameters.Person,new DateOnly(2000, 1, 1));
			Assert.AreEqual(SchedulePartView.None, part.SignificantPart());
		}

		#endregion

		private static DateTimePeriod createPeriod(TimeSpan span)
		{
			var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return new DateTimePeriod(date.Add(span), date.Add(span).AddHours(1));
		}

		[Test]
		public void VerifyDeleteOvertime()
		{
			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(_target.Person, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);
			_target.CreateAndAddOvertime(activity, period, definitionSet);
 
			Assert.AreEqual(1, _target.PersonAssignmentCollection()[0].OvertimeLayers().Count());
			_target.DeleteOvertime();
			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);

			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			_target.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreEqual(1, _target.PersonAssignmentCollection()[0].OvertimeLayers().Count());
			_target.DeleteOvertime();
			Assert.AreEqual(0, _target.PersonAssignmentCollection().Count);
		}

		[Test]
		public void VerifyDeletePersonalStuff()
		{
			SetupForMergeTests();

			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			//personal shift + overtime shift
			source.Add(personAssignmentSource);
			source.CreateAndAddOvertime(activity, period, definitionSet);
			source.DeleteMainShift(source);
			source.DeletePersonalStuff();
			Assert.IsNotEmpty(source.PersonAssignmentCollection());
			
			//personal shift
			SetupForMergeTests();
			source.Add(personAssignmentSource);
			source.DeleteMainShift(source);
			source.DeletePersonalStuff();
			Assert.IsEmpty(source.PersonAssignmentCollection());
		}

		[Test]
		public void VerifyDeleteMainShift()
		{
			SetupForMergeTests();

			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			//mainshift + personal shift
			source.Add(personAssignmentSource);
			source.DeleteMainShift(source);
			Assert.IsNull(source.PersonAssignmentCollection()[0].ShiftCategory);
			Assert.AreEqual(1, source.PersonAssignmentCollection()[0].PersonalLayers().Count());

			//mainshift + overtime shift
			var options = new DeleteOption {PersonalShift = true};
			((ExtractedSchedule)source).Remove(options);
			source.CreateAndAddOvertime(activity, period, definitionSet);
			var mainShift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();
			new EditableShiftMapper().SetMainShiftLayers(_target.PersonAssignmentCollection()[0], mainShift);
			source.DeleteMainShift(source);
            Assert.AreEqual(1, source.PersonAssignmentCollection().Count);

			//no mainshift and no overtime shift
			source.DeleteOvertime();
			source.DeleteMainShift(source);
            Assert.AreEqual(0, source.PersonAssignmentCollection().Count);
		}


		[Test]
		public void VerifyMergeOvertime()
		{
			SetupForMergeTests();

			IMultiplicatorDefinitionSet definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			IActivity activity = ActivityFactory.CreateActivity("activity");
			DateTime start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(start, end);

			source.CreateAndAddOvertime(activity, period, definitionSet);
			Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);
			((ExtractedSchedule)destination).MergeOvertime(source);
			Assert.AreEqual(0, destination.PersonAssignmentCollection().Count);

			PersonFactory.AddDefinitionSetToPerson(person2, definitionSet);
			((ExtractedSchedule)destination).MergeOvertime(source);
			Assert.AreEqual(start.Hour, destination.PersonAssignmentCollection()[0].OvertimeLayers().Single().Period.StartDateTime.Hour);
			Assert.AreEqual(end.Hour, destination.PersonAssignmentCollection()[0].OvertimeLayers().Single().Period.EndDateTime.Hour);
 
		}

		[Test]
		public void VerifyRemoveOptions()
		{
			DeleteOption options = new DeleteOption();

			//absence
			SetupForRemoveOptionsTest();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.Absence = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//dayoff
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.DayOff = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//mainshift
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.MainShift = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//overtime
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.Overtime = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//personalshift
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.PersonalShift = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//preference
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.Preference = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//studentavailability
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.StudentAvailability = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//multiple random
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.StudentAvailability = true;
			options.Overtime = true;
			options.Absence = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//all
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			options.Absence = true;
			options.DayOff = true;
			options.MainShift = true;
			options.Overtime = true;
			options.PersonalShift = true;
			options.Preference = true;
			options.StudentAvailability = true;
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));

			//none
			SetupForRemoveOptionsTest();
			options = new DeleteOption();
			Assert.IsTrue(AssertAllStartDataForRemoveOptionsTestIsSet(source));
			((ExtractedSchedule)source).Remove(options);
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(source, options));
			Assert.IsTrue(AssertAllEndDataForRemoveOptionsTestShouldBePresent(source, options));
			
		}

		private static bool AssertAllStartDataForRemoveOptionsTestIsSet(IScheduleDay part)
		{
			return part.PersonAssignmentCollection().Count == 1 &&
				   part.PersonDayOffCollection().Count == 1 &&
				   part.PersonAbsenceCollection().Count == 1 &&
				   part.PersonAssignmentCollection()[0].PersonalLayers().Count() == 1 &&
				   part.PersonAssignmentCollection()[0].OvertimeLayers().Count() == 1 &&
				   part.PersonRestrictionCollection().Count == 2;
		}

		private static bool AssertAllEndDataForRemoveOptionsTestShouldNotBePresent(IScheduleDay part, DeleteOption options)
		{
			bool retValue = true;

			//should not be there
			if (options.Absence && part.PersonAbsenceCollection().Count > 0)
				retValue = false;

			if (options.DayOff && part.PersonDayOffCollection().Count > 0)
				retValue = false;

			if (options.MainShift && part.PersonAssignmentCollection().Count > 0 && part.PersonAssignmentCollection()[0].ShiftCategory != null)
				retValue = false;

			if (options.Overtime && part.PersonAssignmentCollection().Count > 0 && part.PersonAssignmentCollection()[0].OvertimeLayers().Any())
				retValue = false;

			if (options.PersonalShift && part.PersonAssignmentCollection().Count > 0 && part.PersonAssignmentCollection()[0].PersonalLayers().Any())
				retValue = false;

			if (options.Preference)
			{
				foreach (IRestrictionBase restriction in part.RestrictionCollection())
				{
					if (restriction is IPreferenceRestriction)
						retValue = false;
				}
			}

			if (options.StudentAvailability)
			{
				foreach (IRestrictionBase restriction in part.RestrictionCollection())
				{
					if (restriction is IStudentAvailabilityRestriction)
						retValue = false;
				}
			}

			return retValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private static bool AssertAllEndDataForRemoveOptionsTestShouldBePresent(IScheduleDay part, DeleteOption options)
		{
			bool retValue = true;

			if (!options.Absence && part.PersonAbsenceCollection().Count == 0)
				retValue = false;

			if (!options.DayOff && part.PersonDayOffCollection().Count == 0)
				retValue = false;

			if ((!options.MainShift && part.PersonAssignmentCollection().Count > 0 && part.PersonAssignmentCollection()[0].ShiftCategory == null) ||
				(!options.MainShift && part.PersonAssignmentCollection().Count == 0))
				retValue = false;

			if ((!options.Overtime && part.PersonAssignmentCollection().Count > 0 && !part.PersonAssignmentCollection()[0].OvertimeLayers().Any()) ||
				(!options.Overtime && part.PersonAssignmentCollection().Count == 0))
				retValue = false;

			if ((!options.PersonalShift && part.PersonAssignmentCollection().Count > 0 && !part.PersonAssignmentCollection()[0].PersonalLayers().Any()) ||
				(!options.PersonalShift && part.PersonAssignmentCollection().Count == 0))
				retValue = false;

			if (!options.Preference)
			{
				retValue = false;

				foreach (IRestrictionBase restriction in part.RestrictionCollection())
				{
					if (restriction is IPreferenceRestriction)
						retValue = true;
				}
			}

			if (!options.StudentAvailability)
			{
				retValue = false;

				foreach (IRestrictionBase restriction in part.RestrictionCollection())
				{
					if (restriction is IStudentAvailabilityRestriction)
						retValue = true;
				}
			}

			return retValue;
		}

        [Test]
        public void ShouldReturnSignificantPartServiceForDisplay()
        {
            Assert.That(_target.SignificantPartForDisplay(),Is.Not.Null);
        }

        [Test]
        public void ShouldNotSplitAbsenceWithSamePeriodAsAssignment()
        {
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
            var dateOnly = new DateOnly(2011, 1, 1);
            var scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, dateOnly);
            Assert.AreEqual(NullScheduleTag.Instance, scheduleDay.ScheduleTag());
        }

        [Test]
        public void ShouldReturnCorrectScheduleTagIfSet()
        {
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
			IActivity activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod1 = new DateTimePeriod(start, end);
			var dateTimePeriod2 = new DateTimePeriod(end.AddHours(1), end.AddHours(2));

			_target = ExtractedSchedule.CreateScheduleDay(dic, parameters.Person, new DateOnly(2000, 1, 1));
			_target.CreateAndAddPersonalActivity(activity, dateTimePeriod1);
			_target.CreateAndAddPersonalActivity(activity, dateTimePeriod2);

			Assert.AreEqual(1, _target.PersonAssignmentCollection().Count);
			Assert.AreEqual(2, _target.PersonAssignmentCollection()[0].PersonalLayers().Count());
			Assert.AreEqual(dateTimePeriod1, _target.PersonAssignmentCollection()[0].PersonalLayers().First().Period);
			Assert.AreEqual(dateTimePeriod2, _target.PersonAssignmentCollection()[0].PersonalLayers().Last().Period);
		}
	}
}
