using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class MergeScheduleDayTest
	{
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		private readonly DateTimePeriod period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc));
		private readonly DateTimePeriod period2 = new DateTimePeriod(new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 10, 1, 17, 0, 0, DateTimeKind.Utc));
		private readonly DateTimePeriod period3 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc));
		private readonly TimeZoneInfo timeZoneInfoTokyo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
		private readonly DateTimePeriod rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);

		[Test]
		public void ShouldSplitAbsenceWhenMergingMainShift()
		{
			var dateBefore = new DateOnly(2018, 10, 1);
			var date = dateBefore.AddDays(1);
			var dateSource = dateBefore.AddDays(4);
			var scenario = new Scenario().WithId();
			var activity = new Activity().WithId();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(new ContractWithMaximumTolerance());
			var expectedPeriod = dateBefore.ToDateTimePeriod(new TimePeriod(0, 27), TimeZoneInfo.Utc);
			var personAssignmentBefore = new PersonAssignment(agent, scenario, dateBefore).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignment = new PersonAssignment(agent, scenario, date).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAssignmentSource = new PersonAssignment(agent, scenario, dateSource).ShiftCategory(new ShiftCategory().WithId()).WithLayer(activity, new TimePeriod(17, 27));
			var personAbsence = new PersonAbsence(agent, scenario, new AbsenceLayer(new Absence().WithId(), dateBefore.ToDateTimePeriod(new TimePeriod(0, 51), TimeZoneInfo.Utc)));
			var data = new List<IPersistableScheduleData> { personAssignmentBefore, personAssignment, personAssignmentSource, personAbsence };
			var stateHolder = SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateBefore, 1), new[] { agent }, data, Enumerable.Empty<ISkillDay>());
			var destination = stateHolder.Schedules[agent].ScheduledDay(date);
			var source = stateHolder.Schedules[agent].ScheduledDay(dateSource);

			destination.Merge(source, false, true, new FakeTimeZoneGuard());

			destination.PersonAbsenceCollection().Single().Period.Should().Be.EqualTo(expectedPeriod);
		}


		[Test]
		public void VerifyMergeEmptyDay()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			destination.Merge(source, false, new FakeTimeZoneGuard());

			Assert.IsTrue(destination.HasDayOff());
			Assert.AreEqual(1, destination.PersonAbsenceCollection().Length);
			Assert.IsNotNull(destination.PersonAssignment());
		}

		[Test]
		public void VerifyMergeDayOff()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			source.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			var authorization = MockRepository.GenerateMock<IAuthorization>();

			authorization.Stub(x => x.IsPermitted("")).Repeat.Once().IgnoreArguments().Return(false);
			authorization.Stub(x => x.IsPermitted("")).IgnoreArguments().Return(true);

			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				destination.Merge(source, false, new FakeTimeZoneGuard());
				Assert.IsTrue(destination.HasDayOff());
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Length);
				Assert.IsNotNull(destination.PersonAssignment());

				destination.Merge(source, false, new FakeTimeZoneGuard());
				Assert.IsTrue(destination.HasDayOff());
				Assert.AreEqual(1, destination.PersonAbsenceCollection(true).Length);
				Assert.IsNotNull(destination.PersonAssignment());
			}
		}

		[Test]
		public void VerifyMergeDayOffNoPermissionModifyAssignment()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			using (CurrentAuthorization.ThreadlyUse(MockRepository.GenerateMock<IAuthorization>()))
			{
				destination.Merge(source, false, new FakeTimeZoneGuard());
				Assert.IsNotNull(destination.PersonAssignment());
			}
		}

		[Test]
		public void VerifyMergeDayOffNoPermissionModifyAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			source.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());
			destination.Add(personAbsenceDest);

			var authorization = MockRepository.GenerateMock<IAuthorization>();

			authorization.Stub(x => x.IsPermitted("")).Repeat.Once().IgnoreArguments().Return(true);
			authorization.Stub(x => x.IsPermitted("")).IgnoreArguments().Return(false);
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				destination.Merge(source, false, new FakeTimeZoneGuard());
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Length);
			}
		}

		[Test]
		public void ShouldNotClearAbsenceDayAfterAddingDayOff()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			source.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());

			var periodDayAfter = new DateTimePeriod(new DateTime(2000, 1, 4, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2000, 1, 4, 17, 0, 0, DateTimeKind.Utc));

			IPersonAbsence personAbsenceDayAfter = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), periodDayAfter));

			destination.Add(personAbsenceDayAfter);

			destination.Merge(source, false, new FakeTimeZoneGuard());
			Assert.IsTrue(destination.PersistableScheduleDataCollection().Contains(personAbsenceDayAfter));
		}

		[Test]
		public void VerifyCreateAndAddActivityOnDayOff()
		{
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			destination.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());
			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			var authorization = MockRepository.GenerateMock<IAuthorization>();
			authorization.Stub(x => x.IsPermitted("")).IgnoreArguments().Return(true);
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				destination.CreateAndAddActivity(ActivityFactory.CreateActivity("activity"), destination.Period, shiftCategory);
				Assert.IsFalse(destination.HasDayOff());
			}
		}

		[Test]
		public void VerifyCreateAndAddActivityOnFullDayAbsence()
		{
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), new FullPermission());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			destination.Add(personAbsenceDest);

			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			destination.CreateAndAddActivity(ActivityFactory.CreateActivity("activity"), destination.Period, shiftCategory);
			Assert.AreEqual(0, destination.PersonAbsenceCollection().Length);
		}

		[Test]
		public void VerifyMergePreference()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceRestriction preferenceRestriction2 = new PreferenceRestriction();
			preferenceRestriction.SetId(Guid.NewGuid());
			preferenceRestriction2.SetId(Guid.NewGuid());
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), preferenceRestriction);
			IPreferenceDay preferenceDay2 = new PreferenceDay(destination.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), preferenceRestriction2);

			source.Add(preferenceDay);
			destination.Add(preferenceDay2);

			destination.Merge(source, false, new FakeTimeZoneGuard());

			Assert.AreEqual(1, ((IList<IRestrictionBase>)destination.RestrictionCollection()).Count);
			Assert.IsTrue(((IList<IRestrictionBase>)destination.RestrictionCollection())[0] is PreferenceRestriction);

			foreach (IPreferenceDay prefDay in destination.PersistableScheduleDataCollection())
			{
				Assert.AreEqual(destination.Period.StartDateTimeLocal(timeZoneInfoTokyo).Date, prefDay.RestrictionDate.Date);
				Assert.IsNull(prefDay.Restriction.Id);
			}
		}

		[Test]
		public void VerifyDeletePreference()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), preferenceRestriction);

			source.Add(preferenceDay);

			source.DeletePreferenceRestriction();

			Assert.IsTrue(((IList<IRestrictionBase>)source.RestrictionCollection()).Count == 0);
		}

		[Test]
		public void VerifyDeleteNote()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			INote note = new Note(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), source.Scenario, "Oh my God");
			source.Add(note);
			Assert.IsTrue(((IList<INote>)source.NoteCollection()).Count == 1);
			source.DeleteNote();

			Assert.IsTrue(((IList<INote>)source.NoteCollection()).Count == 0);
		}

		[Test]
		public void VerifyMergeStudentAvailability()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			studentAvailabilityRestriction.SetId(Guid.NewGuid());
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), list);

			source.Add(studentAvailabilityDay);

			destination.Merge(source, false, new FakeTimeZoneGuard());

			Assert.IsTrue(((IList<IRestrictionBase>)destination.RestrictionCollection())[0] is StudentAvailabilityRestriction);

			foreach (IStudentAvailabilityDay studDay in destination.PersistableScheduleDataCollection())
			{
				Assert.AreEqual(destination.Period.StartDateTimeLocal(timeZoneInfoTokyo).Date, studDay.RestrictionDate.Date);
				Assert.IsNull(studDay.RestrictionCollection.First().Id);
			}
		}

		[Test]
		public void VerifyDeleteStudentAvailability()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), list);

			source.Add(studentAvailabilityDay);

			source.DeleteStudentAvailabilityRestriction();

			Assert.IsTrue(((IList<IRestrictionBase>)source.RestrictionCollection()).Count == 0);
		}

		[Test]
		public void VerifyDeleteOvertimeAvailability()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var overtimeAvailabilityDay = new OvertimeAvailability(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), TimeSpan.FromHours(17), TimeSpan.FromHours(19));

			source.Add(overtimeAvailabilityDay);

			source.DeleteOvertimeAvailability();

			Assert.IsTrue(source.OvertimeAvailablityCollection().Length == 0);
		}

		[Test]
		public void VerifyDeleteAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			DateTime absStart1 = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			DateTime absEnd1 = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			DateTime absStart2 = new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc);
			DateTime absEnd2 = new DateTime(2000, 1, 1, 14, 0, 0, DateTimeKind.Utc);

			DateTimePeriod absPeriod1 = new DateTimePeriod(absStart1, absEnd1);
			DateTimePeriod absPeriod2 = new DateTimePeriod(absStart2, absEnd2);
			DateTimePeriod assPeriod = new DateTimePeriod(absStart1, absEnd2);

			IAbsenceLayer absenceLayer1 = new AbsenceLayer(new Absence(), absPeriod1);
			IAbsenceLayer absenceLayer2 = new AbsenceLayer(new Absence(), absPeriod2);

			IPersonAbsence personAbsence1 = new PersonAbsence(person1, scenario, absenceLayer1);
			IPersonAbsence personAbsence2 = new PersonAbsence(person1, scenario, absenceLayer2);
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person1, scenario, assPeriod);

			//without assignment
			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 2);
			source.DeleteAbsence(true); //special delete
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 0);

			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 2);
			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 1);

			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 0);

			//with assignment
			source.Add(ass);
			source.Add(personAbsence1);
			source.Add(personAbsence2);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 2);
			source.DeleteAbsence(false);
			Assert.IsTrue(source.PersonAbsenceCollection().Length == 1);
		}

		[Test]
		public void ShouldNotDeleteAbsenceOnNextDay()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

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
		public void ShouldDeleteOldestAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), new Dictionary<IPerson, IScheduleRange>());
			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2000,1,1), CurrentAuthorization.Make());
			IPersonAbsence personAbsence = new PersonAbsence(person1, scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2000,1,2)));
			IPersonAbsence oldestAbsence = new PersonAbsence(person1, scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2000,1,1,2000,1,2)));
			oldestAbsence.LastChange = personAbsence.LastChange.Value.AddDays(-1);		
			source.Add(personAbsence);
			source.Add(oldestAbsence);

			source.DeleteAbsence(false);

			Assert.AreEqual(1, source.PersistableScheduleDataCollection().Count());
			var a = source.PersonAbsenceCollection().Single();
			Assert.AreEqual(personAbsence, a);
		}

		[Test]
		public void VerifyMergeFullDayAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			var personAssignmentSource = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person1,
							scenario, ActivityFactory.CreateActivity("sdfsdf"), period1, ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			var personAbsenceSource = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), parameters1.Period));
			var personAbsenceSource2 = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs2"), parameters1.Period));

			source.Add(personAbsenceSource);
			source.Add(personAbsenceSource2);
			source.Add(personAssignmentSource);

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			((ExtractedSchedule)destination).MergeAbsences(source, true);

			Assert.IsTrue(destination.HasDayOff());
			Assert.AreEqual(3, destination.PersonAbsenceCollection().Length);
			Assert.IsNotNull(destination.PersonAssignment());
		}

		[Test]
		public void ShouldSetLastChangeWhenMergeFullDayAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));
			var personAbsenceSource = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), parameters1.Period));
			
			personAbsenceSource.LastChange = DateTime.UtcNow - TimeSpan.FromHours(1);
			personAbsenceDest.LastChange = personAbsenceSource.LastChange;

			source.Add(personAbsenceSource);
			destination.Add(personAbsenceDest);

			((ExtractedSchedule)destination).MergeAbsences(source, true);

			Assert.IsTrue(destination.PersonAbsenceCollection()[1].LastChange > destination.PersonAbsenceCollection()[0].LastChange);

		}

		[Test]
		public void ShouldPasteAbsenceFromSourceWithContractDayOff()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAbsenceSource = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), parameters1.Period));
			var personAbsenceSource2 = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs2"), parameters1.Period));

			var absenceService = MockRepository.GenerateMock<ISignificantPartService>(); //Service for easier testing with Significantpart
			absenceService.Stub(x => x.SignificantPart()).Return(SchedulePartView.Absence);

			source.Add(personAbsenceSource);
			source.Add(personAbsenceSource2);

			destination.Add(personAbsenceDest);

			((ExtractedSchedule)source).ServiceForSignificantPart = absenceService; //Setup for returning Absence;

			Assert.AreEqual(1, destination.PersonAbsenceCollection().Length);
			destination.Merge(source, false, new FakeTimeZoneGuard());
			Assert.AreEqual(2, destination.PersonAbsenceCollection().Length);
		}

		[Test]
		public void VerifyMergeAbsence()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			var period = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			IPersonAbsence personAbsenceNotFullDay = PersonAbsenceFactory.CreatePersonAbsence(person1, scenario, period);

			var fullDayAbsenceService = MockRepository.GenerateMock<ISignificantPartService>();
			fullDayAbsenceService.Stub(x => x.SignificantPart()).Return(SchedulePartView.FullDayAbsence);

			source.Add(personAbsenceNotFullDay);

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			((ExtractedSchedule)source).ServiceForSignificantPart = fullDayAbsenceService;

			destination.Merge(source, false, new FakeTimeZoneGuard());

			Assert.IsTrue(destination.HasDayOff());
			Assert.AreEqual(2, destination.PersonAbsenceCollection().Length);
			Assert.IsNotNull(destination.PersonAssignment());
		}

		[Test]
		public void VerifyMergeAbsenceDaylightSaving()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var date = new DateOnly(2010, 3, 22);
			var dateTimeSourceStart = new DateTime(2010, 3, 22, 8, 0, 0, DateTimeKind.Utc);

			var dateTimeAbsenceStart = dateTimeSourceStart.AddHours(2);
			var dateTimeAbsenceEnd = dateTimeAbsenceStart.AddHours(2);

			var periodAbsence = new DateTimePeriod(dateTimeAbsenceStart, dateTimeAbsenceEnd);

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, date, CurrentAuthorization.Make());
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, date.AddDays(10), CurrentAuthorization.Make());

			var absenceService = MockRepository.GenerateMock<ISignificantPartService>();
			((ExtractedSchedule)source).ServiceForSignificantPart = absenceService;

			absenceService.Stub(x => x.SignificantPart()).Return(SchedulePartView.Absence);

			IPersonAbsence absence = PersonAbsenceFactory.CreatePersonAbsence(person1, scenario, periodAbsence);
			source.Add(absence);

			destination.Merge(source, false, new FakeTimeZoneGuard());
			Assert.AreEqual(periodAbsence.StartDateTimeLocal(TimeZoneInfoFactory.StockholmTimeZoneInfo()).TimeOfDay, destination.PersonAbsenceCollection()[0].Period.StartDateTimeLocal(TimeZoneInfoFactory.StockholmTimeZoneInfo()).TimeOfDay);
		}

		[Test]
		public void VerifyCalculateDiffPasteFromWinterTimeToWinterTime()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var expectedDiff = new TimeSpan(2, 0, 0, 0);
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23), CurrentAuthorization.Make());
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 25), CurrentAuthorization.Make());
			TimeSpan diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);
		}

		[Test]
		public void VerifyCalculateDiffPasteFromWinterTimeToDaylightSavingsStart()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var expectedDiff = new TimeSpan(5, 0, 0, 0);
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23), CurrentAuthorization.Make());
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 28), CurrentAuthorization.Make());
			var diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);
		}

		[Test]
		public void VerifyCalculateDiffPasteFromWinterTimeToSummerTime()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var expectedDiff = new TimeSpan(6, 23, 0, 0);
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23), CurrentAuthorization.Make());
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 30), CurrentAuthorization.Make());
			var diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);
		}

		[Test]
		public void VerifyCalculateDiffPasteFromSummerTimeToWinterTime()
		{
			var person1 = PersonFactory.CreatePerson();
			person1.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var expectedDiff = new TimeSpan(-6, -23, 0, 0);
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 30), CurrentAuthorization.Make());
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23), CurrentAuthorization.Make());
			var diff = testDestination.CalculatePeriodOffset(testSource.Period);
			Assert.AreEqual(expectedDiff, diff);
		}

		[Test]
		public void VerifyMergeMainShift()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			var personAssignmentSource = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person1,
							scenario, ActivityFactory.CreateActivity("sdfsdf"), period1, ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			source.Add(personAssignmentSource);

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);

			var authorization = MockRepository.GenerateMock<IAuthorization>();
			authorization.Stub(x => x.IsPermitted("")).Repeat.Once().IgnoreArguments().Return(false);
			authorization.Stub(x => x.IsPermitted("")).IgnoreArguments().Return(true);

			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				destination.Merge(source, false, new FakeTimeZoneGuard());
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Length);
				Assert.IsNotNull(destination.PersonAssignment());

				destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());
				destination.Merge(source, false, new FakeTimeZoneGuard());
				//assert dayoff is still there - NOPE! changed
				Assert.IsFalse(destination.HasDayOff());
				//assert absence is splitted
				Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Length);
				//assert we still have 1 assignment
				Assert.IsNotNull(destination.PersonAssignment());
				//assert destination have got the source period(Time)
				Assert.AreEqual(
					source.PersonAssignment().MainActivities().First().Period.TimePeriod(
						TimeZoneInfoFactory.StockholmTimeZoneInfo()),
					destination.PersonAssignment().MainActivities().First().Period.TimePeriod(
						TimeZoneInfoFactory.StockholmTimeZoneInfo()));

				//clear assignments in destination
				destination.Clear<IPersonAssignment>();
				//merge
				destination.Merge(source, false, new FakeTimeZoneGuard());
				//assert a new assignment is created in destination
				Assert.IsNotNull(destination.PersonAssignment());
				//assert dayoff is removed
				Assert.IsFalse(destination.HasDayOff());
			}
		}

		[Test]
		public void VerifyMergePersonalStuff()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			//create personassignment with no mainshift
			IPersonAssignment newPersonAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);
			//add personal layer to assignment
			newPersonAssignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), period3);
			//add assignment to source
			source.Add(newPersonAssignment);

			destination.Merge(source, false, new FakeTimeZoneGuard());

			//assert we still have 1 assignment in destination
			Assert.IsNotNull(destination.PersonAssignment());
			//assert the personal shift was added
			Assert.AreEqual(2, destination.PersonAssignment().PersonalActivities().Count());

			//clear assignments in destination
			destination.Clear<IPersonAssignment>();
			//merge
			destination.Merge(source, false, new FakeTimeZoneGuard());

			//assert the personal shift was added and a new assignment was added
			Assert.IsNotNull(destination.PersonAssignment());
		}

		[Test]
		public void ShouldConsiderDaylightSavingsWhenMergePersonalStuff()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var sourcePeriod =  new DateTimePeriod(2016, 3, 25, 23, 2016, 3, 26,23);
			var destinationPeriod = new DateTimePeriod(2016, 3, 26, 23, 2016, 3, 27, 22);
			var shiftSourcePeriod = new DateTimePeriod(2016, 3, 26, 9, 2016, 3, 26, 10);

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(sourcePeriod.StartDateTime), CurrentAuthorization.Make());
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(destinationPeriod.StartDateTime), CurrentAuthorization.Make());
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);

			personAssignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), shiftSourcePeriod);
			source.Add(personAssignment);

			destination.Merge(source, false, new FakeTimeZoneGuard());
			destination.PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(shiftSourcePeriod.StartDateTime.Hour);
		}

		[Test]
		public void ShouldHandleDaylightSavingsAutumnWhenMergePersonalStuff()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var sourcePeriod = new DateTimePeriod(2016, 10, 24, 22, 2016, 10, 25, 22);
			var destinationPeriod = new DateTimePeriod(2016, 10, 31, 22, 2016, 11, 01, 22);
			var shiftSourcePeriod = new DateTimePeriod(2016, 10, 25, 9, 2016, 10, 25, 10);

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(sourcePeriod.StartDateTime), CurrentAuthorization.Make());
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(destinationPeriod.StartDateTime), CurrentAuthorization.Make());
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);

			personAssignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), shiftSourcePeriod);
			source.Add(personAssignment);

			destination.Merge(source, false, new FakeTimeZoneGuard());
			destination.PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(shiftSourcePeriod.StartDateTime.Hour);
		}

		[Test]
		public void VerifyMergingWithAnotherTimeZoneNotChangesDateOnlyAsPeriod()
		{
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			IPerson sourcePerson = PersonFactory.CreatePerson();
			sourcePerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

			IPerson targetPersonInSameTimezone = PersonFactory.CreatePerson();
			targetPersonInSameTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.HawaiiTimeZoneInfo());

			IPerson targetPersonInDifferentTimezone = PersonFactory.CreatePerson();
			targetPersonInDifferentTimezone.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.SingaporeTimeZoneInfo());

			DateOnly day = new DateOnly(2011, 02, 10);
			DateTime start = new DateTime(2011, 2, 10, 16, 0, 0, 0, DateTimeKind.Utc);
			DateTime end = new DateTime(2011, 2, 10, 20, 0, 0, 0, DateTimeKind.Utc);

			IScheduleDay sourceDay = ExtractedSchedule.CreateScheduleDay(dic, sourcePerson, day, CurrentAuthorization.Make());
			IActivity activity = ActivityFactory.CreateActivity("Test");

			DateTimePeriod period = new DateTimePeriod(start, end);
			ShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("Test");
			var mainShift = EditableShiftFactory.CreateEditorShift(activity, period, shiftCategory);
			sourceDay.AddMainShift(mainShift);

			IScheduleDay targetDaySameTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInSameTimezone, day, CurrentAuthorization.Make());
			IScheduleDay targetDayDifferentTimezone = ExtractedSchedule.CreateScheduleDay(dic, targetPersonInDifferentTimezone, day, CurrentAuthorization.Make());

			targetDaySameTimezone.Merge(sourceDay, false, false, new FakeTimeZoneGuard());

			targetDayDifferentTimezone.Merge(sourceDay, false, false, new FakeTimeZoneGuard());

			DateOnly expectedDateOnly = day;
			Assert.AreEqual(expectedDateOnly, targetDaySameTimezone.DateOnlyAsPeriod.DateOnly);

			expectedDateOnly = day;
			Assert.AreEqual(expectedDateOnly, targetDayDifferentTimezone.DateOnlyAsPeriod.DateOnly);
		}

		[Test]
		public void ShouldMergeMainShiftDaylightSavingsDay()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2014, 3, 27, 8, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2014, 3, 27, 10, 0, 0, DateTimeKind.Utc);
			var expectedPeriod = new DateTimePeriod(2014, 3, 30, 8, 2014, 3, 30, 10);
			var shiftCategory = new ShiftCategory("shiftCategory");

			var dateTimePeriod1 = new DateTimePeriod(start, end);

			var sourceDay = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2014, 3, 27), new FullPermission());
			var targetDay = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2014, 3, 30), new FullPermission());
			sourceDay.CreateAndAddActivity(activity, dateTimePeriod1, shiftCategory);

			targetDay.Merge(sourceDay, false, new FakeTimeZoneGuard());
			Assert.AreEqual(expectedPeriod, targetDay.PersonAssignment().Period);
		}

		[Test]
		public void VerifyMergeOvertime()
		{
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime), CurrentAuthorization.Make());

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime), CurrentAuthorization.Make());

			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			source.CreateAndAddOvertime(activity, period, definitionSet);
			destination.PersonAssignment().Should().Be.Null();
			((ExtractedSchedule)destination).MergeOvertime(source, new FakeTimeZoneGuard());
			destination.PersonAssignment().Should().Be.Null();

			PersonFactory.AddDefinitionSetToPerson(person2, definitionSet);
			((ExtractedSchedule)destination).MergeOvertime(source, new FakeTimeZoneGuard());
			Assert.AreEqual(start.Hour, destination.PersonAssignment().OvertimeActivities().Single().Period.StartDateTime.Hour);
			Assert.AreEqual(end.Hour, destination.PersonAssignment().OvertimeActivities().Single().Period.EndDateTime.Hour);

		}
	}
}