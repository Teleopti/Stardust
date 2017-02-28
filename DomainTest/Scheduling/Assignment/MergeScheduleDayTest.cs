using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestWithStaticDependenciesAvoidUse]
	public class MergeScheduleDayTest
	{
		private readonly DateTimePeriod period1 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc));
		private readonly DateTimePeriod period2 = new DateTimePeriod(new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 10, 1, 17, 0, 0, DateTimeKind.Utc));
		private readonly DateTimePeriod period3 = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 15, 0, 0, DateTimeKind.Utc));
		private readonly TimeZoneInfo timeZoneInfoTokyo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
		private readonly DateTimePeriod rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			destination.Add(personAbsenceDest);
			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			destination.Merge(source, false);

			Assert.IsTrue(destination.HasDayOff());
			Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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
				destination.Merge(source, false);
				Assert.IsTrue(destination.HasDayOff());
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
				Assert.IsNotNull(destination.PersonAssignment());

				//verify that day off is pasted, absencense is splitted and assignment is removed
				destination.Merge(source, false);
				Assert.IsTrue(destination.HasDayOff());
				Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var personAssignmentDest = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person2,
				scenario, ActivityFactory.CreateActivity("sdfsdf"), period3.MovePeriod(TimeSpan.FromDays(1)), ShiftCategoryFactory.CreateShiftCategory("shiftCategory"));

			destination.Add(personAssignmentDest);
			destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());

			using (CurrentAuthorization.ThreadlyUse(MockRepository.GenerateMock<IAuthorization>()))
			{
				destination.Merge(source, false);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			source.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());
			destination.Add(personAbsenceDest);

			var authorization = MockRepository.GenerateMock<IAuthorization>();

			authorization.Stub(x => x.IsPermitted("")).Repeat.Once().IgnoreArguments().Return(true);
			authorization.Stub(x => x.IsPermitted("")).IgnoreArguments().Return(false);
			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				destination.Merge(source, false);
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			source.PersonAssignment(true).SetDayOff(DayOffFactory.CreateDayOff());

			var periodDayAfter = new DateTimePeriod(new DateTime(2000, 1, 4, 8, 0, 0, DateTimeKind.Utc),
				new DateTime(2000, 1, 4, 17, 0, 0, DateTimeKind.Utc));

			IPersonAbsence personAbsenceDayAfter = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), periodDayAfter));

			destination.Add(personAbsenceDayAfter);

			destination.Merge(source, false);
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

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			destination.Add(personAbsenceDest);

			IShiftCategory shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");

			destination.CreateAndAddActivity(ActivityFactory.CreateActivity("activity"), destination.Period, shiftCategory);
			Assert.AreEqual(0, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			IPreferenceRestriction preferenceRestriction = new PreferenceRestriction();
			IPreferenceRestriction preferenceRestriction2 = new PreferenceRestriction();
			preferenceRestriction.SetId(Guid.NewGuid());
			preferenceRestriction2.SetId(Guid.NewGuid());
			IPreferenceDay preferenceDay = new PreferenceDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), preferenceRestriction);
			IPreferenceDay preferenceDay2 = new PreferenceDay(destination.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), preferenceRestriction2);

			source.Add(preferenceDay);
			destination.Add(preferenceDay2);

			destination.Merge(source, false);

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			IStudentAvailabilityRestriction studentAvailabilityRestriction = new StudentAvailabilityRestriction();
			studentAvailabilityRestriction.SetId(Guid.NewGuid());
			IList<IStudentAvailabilityRestriction> list = new List<IStudentAvailabilityRestriction>();
			list.Add(studentAvailabilityRestriction);
			IStudentAvailabilityDay studentAvailabilityDay = new StudentAvailabilityDay(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), list);

			source.Add(studentAvailabilityDay);

			destination.Merge(source, false);

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var overtimeAvailabilityDay = new OvertimeAvailability(source.Person, new DateOnly(source.Period.StartDateTimeLocal(timeZoneInfoTokyo)), TimeSpan.FromHours(17), TimeSpan.FromHours(19));

			source.Add(overtimeAvailabilityDay);

			source.DeleteOvertimeAvailability();

			Assert.IsTrue(source.OvertimeAvailablityCollection().Count == 0);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

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
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);
			
			var parameters1 =
				new ScheduleParameters(scenario, person1, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));

			var parameters2 =
				new ScheduleParameters(scenario, person2, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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
			Assert.AreEqual(3, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var personAbsenceDest = new PersonAbsence(person2, scenario, new AbsenceLayer(new Absence(), period2));

			var personAbsenceSource = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs"), parameters1.Period));
			var personAbsenceSource2 = new PersonAbsence(person1, scenario, new AbsenceLayer(AbsenceFactory.CreateAbsence("abs2"), parameters1.Period));

			var absenceService = MockRepository.GenerateMock<ISignificantPartService>(); //Service for easier testing with Significantpart
			absenceService.Stub(x => x.SignificantPart()).Return(SchedulePartView.Absence);

			source.Add(personAbsenceSource);
			source.Add(personAbsenceSource2);

			destination.Add(personAbsenceDest);

			((ExtractedSchedule)source).ServiceForSignificantPart = absenceService; //Setup for returning Absence;

			Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
			destination.Merge(source, false);
			Assert.AreEqual(2, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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

			destination.Merge(source, false);

			Assert.IsTrue(destination.HasDayOff());
			Assert.AreEqual(2, destination.PersonAbsenceCollection().Count);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, date);
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, date.AddDays(10));

			var absenceService = MockRepository.GenerateMock<ISignificantPartService>();
			((ExtractedSchedule)source).ServiceForSignificantPart = absenceService;

			absenceService.Stub(x => x.SignificantPart()).Return(SchedulePartView.Absence);

			IPersonAbsence absence = PersonAbsenceFactory.CreatePersonAbsence(person1, scenario, periodAbsence);
			source.Add(absence);

			destination.Merge(source, false);
			Assert.AreEqual(periodAbsence.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).TimeOfDay, destination.PersonAbsenceCollection()[0].Period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone).TimeOfDay);
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
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23));
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 25));
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
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23));
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 28));
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
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23));
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 30));
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
			var testSource = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 30));
			var testDestination = (ExtractedSchedule)ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2010, 3, 23));
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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
				destination.Merge(source, false);
				Assert.AreEqual(1, destination.PersonAbsenceCollection().Count);
				Assert.IsNotNull(destination.PersonAssignment());

				destination.PersonAssignment().SetDayOff(DayOffFactory.CreateDayOff());
				destination.Merge(source, false);
				//assert dayoff is still there - NOPE! changed
				Assert.IsFalse(destination.HasDayOff());
				//assert absence is splitted
				Assert.AreEqual(2, destination.PersonAbsenceCollection(true).Count);
				//assert we still have 1 assignment
				Assert.IsNotNull(destination.PersonAssignment());
				//assert destination have got the source period(Time)
				Assert.AreEqual(
					source.PersonAssignment().MainActivities().First().Period.TimePeriod(
						TimeZoneHelper.CurrentSessionTimeZone),
					destination.PersonAssignment().MainActivities().First().Period.TimePeriod(
						TimeZoneHelper.CurrentSessionTimeZone));

				//clear assignments in destination
				destination.Clear<IPersonAssignment>();
				//merge
				destination.Merge(source, false);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

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

			destination.Merge(source, false);

			//assert we still have 1 assignment in destination
			Assert.IsNotNull(destination.PersonAssignment());
			//assert the personal shift was added
			Assert.AreEqual(2, destination.PersonAssignment().PersonalActivities().Count());

			//clear assignments in destination
			destination.Clear<IPersonAssignment>();
			//merge
			destination.Merge(source, false);

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

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(sourcePeriod.StartDateTime));
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(destinationPeriod.StartDateTime));
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);

			personAssignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), shiftSourcePeriod);
			source.Add(personAssignment);

			destination.Merge(source, false);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(sourcePeriod.StartDateTime));
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(destinationPeriod.StartDateTime));
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person1, scenario);

			personAssignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), shiftSourcePeriod);
			source.Add(personAssignment);

			destination.Merge(source, false);
			destination.PersonAssignment().Period.StartDateTime.Hour.Should().Be.EqualTo(shiftSourcePeriod.StartDateTime.Hour);
		}

		[Test]
		public void ShouldConsiderDaylightSavingsWhenMergeOvertime()
		{
			var person1 = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
			var dic = new ScheduleDictionaryForTest(scenario, new ScheduleDateTimePeriod(rangePeriod), underlyingDictionary);

			var sourcePeriod = new DateTimePeriod(2016, 3, 25, 23, 2016, 3, 26, 23);
			var destinationPeriod = new DateTimePeriod(2016, 3, 26, 23, 2016, 3, 27, 22);
			var shiftSourcePeriod = new DateTimePeriod(2016, 3, 26, 9, 2016, 3, 26, 10);

			var source = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(sourcePeriod.StartDateTime));
			var destination = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(destinationPeriod.StartDateTime));

			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			var activity = ActivityFactory.CreateActivity("activity");
			source.CreateAndAddOvertime(activity, shiftSourcePeriod, definitionSet);

			((ExtractedSchedule)destination).MergeOvertime(source);
			destination.PersonAssignment().Period.Should().Be.EqualTo(shiftSourcePeriod.MovePeriod(TimeSpan.FromHours(23)));
		}

		[Test]
		public void VerifyCopyPasteToAnotherTimeZoneChangesDateOnlyAsPeriod()
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

			var sourceDay = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2014, 3, 27));
			var targetDay = ExtractedSchedule.CreateScheduleDay(dic, person1, new DateOnly(2014, 3, 30));
			sourceDay.CreateAndAddActivity(activity, dateTimePeriod1, shiftCategory);

			targetDay.Merge(sourceDay, false);
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

			var source = ExtractedSchedule.CreateScheduleDay(dic, parameters1.Person, new DateOnly(parameters1.Period.StartDateTime));

			var destination = ExtractedSchedule.CreateScheduleDay(dic, parameters2.Person, new DateOnly(parameters2.Period.StartDateTime));

			var definitionSet = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(person1, definitionSet);
			var activity = ActivityFactory.CreateActivity("activity");
			var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(start, end);

			source.CreateAndAddOvertime(activity, period, definitionSet);
			destination.PersonAssignment().Should().Be.Null();
			((ExtractedSchedule)destination).MergeOvertime(source);
			destination.PersonAssignment().Should().Be.Null();

			PersonFactory.AddDefinitionSetToPerson(person2, definitionSet);
			((ExtractedSchedule)destination).MergeOvertime(source);
			Assert.AreEqual(start.Hour, destination.PersonAssignment().OvertimeActivities().Single().Period.StartDateTime.Hour);
			Assert.AreEqual(end.Hour, destination.PersonAssignment().OvertimeActivities().Single().Period.EndDateTime.Hour);

		}
	}
}