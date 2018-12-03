using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	public class ScheduleProjectionServiceTest
	{
		private IScheduleDay scheduleDay;
		private ScheduleProjectionService target;
	    private ScheduleDictionary dic;
		private IPersistableScheduleDataPermissionChecker permissionChecker;

		private void setup()
		{
			permissionChecker = new PersistableScheduleDataPermissionChecker(new FullPermission());
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dic = new ScheduleDictionary(new Scenario("sd"), new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2200, 1, 1)), permissionChecker, new FullPermission());
			scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2000, 1, 1), new FullPermission());
			target = new ScheduleProjectionService(scheduleDay, new ProjectionPayloadMerger());
		}

		[Test]
		public void VerifyFakeActivity()
		{
			setup();
			var fake = target.FakeActivity;
			Assert.AreSame(target.FakeActivity, fake);
			Assert.IsTrue(fake.InWorkTime);
			Assert.IsTrue(fake.InPaidTime);
			Assert.IsTrue(fake.InContractTime);
		}

		private IPersonAbsence createPersonAbsence(byte prioAndName, DateTimePeriod period)
		{
			var prioAsString = prioAndName.ToString(CultureInfo.CurrentCulture);
			var abs = AbsenceFactory.CreateAbsence(prioAsString);
			abs.Description = new Description(prioAsString);
			abs.Priority = prioAndName;
			abs.SetId(Guid.NewGuid());
			return new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(abs, period));
		}

		[Test]
		public void VerifyEnumeratorDoesNotMessWithLayers()
		{
			setup();
			var ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, createPeriod(8, 16));
			var pActivity = ActivityFactory.CreateActivity("personal");
			ass1.AddPersonalActivity(pActivity, createPeriod(10,13));
			var abs1 = createPersonAbsence(100, createPeriod(8, 13));
			scheduleDay.Add(ass1);
			scheduleDay.Add(abs1);

			var visualCollection = target.CreateProjection();

			var anotherVisualCollection = visualCollection.FilterLayers(createPeriod(11, 12));
			Assert.AreEqual(1, anotherVisualCollection.Count());
			anotherVisualCollection = visualCollection.FilterLayers(createPeriod(11, 12));
			Assert.AreEqual(1, anotherVisualCollection.Count());
		}

		[Test]
		public void VerifyMultiplePersonAbsencesAndCorrectUnderlyingLayers()
		{
			setup();
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("första"), createPeriod(2, 9));
			ass.AddActivity(new Activity("andra"), createPeriod(11, 19));

			var abs1 = createPersonAbsence(70, createPeriod(-1, 2)); //no
			var abs2 = createPersonAbsence(60, createPeriod(-50, 3));
			var abs3 = createPersonAbsence(50, createPeriod(10, 12)); //no
			var abs4 = createPersonAbsence(40, createPeriod(10, 12));
			var abs5 = createPersonAbsence(30, createPeriod(9, 11));
			var abs6 = createPersonAbsence(20, createPeriod(19, 50)); //no
			var abs7 = createPersonAbsence(10, createPeriod(8, 12));
			
			scheduleDay.Add(ass);
			scheduleDay.Add(abs1);
			scheduleDay.Add(abs2);
			scheduleDay.Add(abs3);
			scheduleDay.Add(abs4);
			scheduleDay.Add(abs5);
			scheduleDay.Add(abs6);
			scheduleDay.Add(abs7);

			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(5, resWrapper.Count);
			var layer = (VisualLayer)resWrapper[0];
			var actLayer1 = ass.MainActivities().First();
			var actLayer2 = ass.MainActivities().Last();

			Assert.AreEqual("60", layer.Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(createPeriod(2, 3), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[1];
			Assert.AreEqual("första", layer.Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(createPeriod(3, 8), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.IsNull(layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[2];
			Assert.AreEqual("10", layer.Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(createPeriod(8, 9), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[3];
			Assert.AreEqual("10", layer.Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(createPeriod(11, 12), layer.Period);
			Assert.AreSame(actLayer2.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[4];
			Assert.AreEqual("andra", layer.Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(createPeriod(12, 19), layer.Period);
			Assert.AreSame(actLayer2.Payload, layer.HighestPriorityActivity);
			Assert.IsNull(layer.HighestPriorityAbsence);
		}

		[Test]
		public void VerifyTotallyOverlapping()
		{
			setup();
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			var abs = createPersonAbsence(100, new DateTimePeriod(1900, 1, 1, 2010, 1, 1));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);

			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			var retLayer = (VisualLayer)resWrapper[0];
			Assert.AreEqual(1, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), retLayer.Period);
			Assert.AreEqual("100", retLayer.Payload.ConfidentialDescription(null).Name);
			Assert.AreSame(ass.MainActivities().First().Payload, retLayer.HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, retLayer.HighestPriorityAbsence);
		}

		[Test]
		public void VerifyResultWhenAbsenceAndContractSaysDayOff()
		{
			setup();
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
				createPeriod(0, 24));
			personAbsence.SetId(Guid.NewGuid());
			scheduleDay.Add(personAbsence);

			addPeriodAndContractToPerson(false);

			var avgWorkTimePerDay = scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.AverageWorkTimePerDay;

			//verify setup
			Assert.IsFalse(
				scheduleDay.Person.Period(new DateOnly(2000, 1, 1))
					.PersonContract.ContractSchedule.IsWorkday(new DateOnly(2000, 1, 1), new DateOnly(2000, 1, 1), DayOfWeek.Monday));
			Assert.IsTrue(avgWorkTimePerDay.Ticks > 0);

			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(1, resWrapper.Count);

			var layer = resWrapper[0];
			Assert.IsFalse(layer.Payload.InContractTime);
			Assert.IsTrue(layer.Payload is IAbsence);
			Assert.AreEqual(layer.Period.ElapsedTime(), avgWorkTimePerDay);
		}

		private TimeSpan addPeriodAndContractToPerson(bool isWorking)
		{
			var schedWeek = new ContractScheduleWeek();
			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999, 1, 1));
			scheduleDay.Person.AddPersonPeriod(period);
			period.PersonContract.ContractSchedule.AddContractScheduleWeek(schedWeek);
			if (isWorking)
			{
				schedWeek.Add(DayOfWeek.Monday, true);
				schedWeek.Add(DayOfWeek.Tuesday, true);
				schedWeek.Add(DayOfWeek.Wednesday, true);
				schedWeek.Add(DayOfWeek.Thursday, true);
				schedWeek.Add(DayOfWeek.Friday, true);
				schedWeek.Add(DayOfWeek.Saturday, true);
				schedWeek.Add(DayOfWeek.Sunday, true);
			}
			return period.PersonContract.AverageWorkTimePerDay;
		}

		[Test]
		public void VerifyHalfOverlapping()
		{
			setup();
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var abs = createPersonAbsence(100, new DateTimePeriod(1900, 1, 1, 2000, 6, 1));

			scheduleDay.Add(ass);
			scheduleDay.Add(abs);

			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(2, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 6, 1), resWrapper[0].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 1, 2001, 1, 1), resWrapper[1].Period);
			Assert.AreEqual("100", resWrapper[0].Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(ass.MainActivities().First().Payload.Description, resWrapper[1].Payload.ConfidentialDescription(null));
			Assert.AreSame(ass.MainActivities().First().Payload, ((VisualLayer)resWrapper[0]).HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)resWrapper[0]).HighestPriorityAbsence);
			Assert.AreSame(ass.MainActivities().First().Payload, ((VisualLayer)resWrapper[1]).HighestPriorityActivity);
			Assert.IsNull(((VisualLayer)resWrapper[1]).HighestPriorityAbsence);
		}

		[Test]
		public void VerifyTinyOverlapping()
		{
			setup();
			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			var abs = createPersonAbsence(100, new DateTimePeriod(2000, 6, 1, 2000, 6, 2));
			
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(3, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 6, 1), resWrapper[0].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 1, 2000, 6, 2), resWrapper[1].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 2, 2001, 1, 1), resWrapper[2].Period);

			var shiftLayer = ass.MainActivities().First();
			Assert.AreEqual(shiftLayer.Payload.Description, resWrapper[0].Payload.ConfidentialDescription(null));
			Assert.AreEqual("100", resWrapper[1].Payload.ConfidentialDescription(null).Name);
			Assert.AreEqual(shiftLayer.Payload.Description, resWrapper[2].Payload.ConfidentialDescription(null));

			Assert.AreSame(shiftLayer.Payload, resWrapper[0].Payload);
			Assert.AreSame(abs.Layer.Payload, resWrapper[1].Payload);
			Assert.AreSame(shiftLayer.Payload, resWrapper[2].Payload);
		}

		[Test]
		public void VerifyOverlappingTwoAssignmentsWithDifferentActivities()
		{
			setup();
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(ActivityFactory.CreateActivity("1"), new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			ass.AddActivity(ActivityFactory.CreateActivity("2"), new DateTimePeriod(2001, 1, 1, 2002, 1, 1));

			var abs = createPersonAbsence(100, new DateTimePeriod(1900, 6, 1, 2100, 6, 2));

			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			
			var res = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(1, res.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), res[0].Period);
			Assert.AreEqual(abs.Layer.Payload, res[0].Payload);

			var layer = (VisualLayer)res[0];
			Assert.AreSame(ass.MainActivities().First().Payload, layer.HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, layer.HighestPriorityAbsence);
		}

		[Test]
		public void VerifyAssignmentAbsenceMeeting()
		{
			setup();
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("1"), createPeriod(0, 1));
			ass.AddActivity(new Activity("2"), createPeriod(1, 20));

			var meeting = createPersonMeeting(createPeriod(2, 6));
			var abs = createPersonAbsence(100, createPeriod(8, 20));

			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			scheduleDay.Add(meeting);

			var res = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(5, res.Count);
			Assert.AreEqual(createPeriod(1, 2), res[1].Period);
			Assert.AreEqual(createPeriod(2, 6), res[2].Period);
			Assert.AreEqual("activity", ((VisualLayer)res[2]).HighestPriorityActivity.Description.Name);
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)res[4]).HighestPriorityAbsence);
		}

		[Test]
		public void VerifyAbsenceHasHigherPriorityThanMeeting()
		{
			setup();
			var abs = createPersonAbsence(100, createPeriod(10, 20));
			var meeting = createPersonMeeting(createPeriod(13, 14));

			scheduleDay.Add(abs);
			scheduleDay.Add(meeting);
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, createPeriod(10, 20)));

			var res = target.CreateProjection();
			Assert.AreEqual(1, res.Count());
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)res.First()).Payload);
			Assert.AreEqual(createPeriod(10, 20), res.First().Period);
		}

		[Test]
		public void VerifyMergeDoesNotMergeDifferentUnderlyingActivityInContractTime()
		{
			setup();
			var phone = new Activity("phone") { InContractTime = true };
			var lunch = new Activity("lunch") { InContractTime = false };
			var sjuk = new Absence { InContractTime = true };

			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(phone, new DateTimePeriod(2000, 1, 1, 2000, 1, 4));
			ass.AddActivity(lunch, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			var abs = new PersonAbsence(ass.Person, ass.Scenario,
				new AbsenceLayer(sjuk, new DateTimePeriod(2000, 1, 1, 2003, 1, 1)));
			abs.SetId(Guid.NewGuid());

			scheduleDay.Add(ass);
			Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.CreateProjection().ContractTime());
			scheduleDay.Add(abs);

			var res = target.CreateProjection();
			Assert.AreEqual(new TimeSpan(2, 0, 0, 0), res.ContractTime());
			Assert.AreEqual(1, res.Count());
			Assert.AreSame(sjuk, new List<IVisualLayer>(res)[0].Payload);
		}

		private IPersonMeeting createPersonMeeting(DateTimePeriod period)
		{
			IMeeting mainMeeting = new Meeting(scheduleDay.Person, new List<IMeetingPerson>(), "subject", "location", "description",
					ActivityFactory.CreateActivity("activity"), scheduleDay.Scenario);

			IPersonMeeting personMeeting = new PersonMeeting(mainMeeting, new MeetingPerson(scheduleDay.Person, true), period);

			personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(scheduleDay.Person, true));

			return personMeeting;
		}

		[Test]
		public void VerifyAbsenceLongerThanOrEqualContractTimeLengthOnNonMainShiftDay()
		{
			setup();
			var perYes = new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc),
				new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc));
			var ass = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(scheduleDay.Person, scheduleDay.Scenario, ActivityFactory.CreateActivity("f"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			scheduleDay.Add(ass);

			var pAbs = createPersonAbsence(100, perYes);
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime =
				new WorkTime(TimeSpan.FromHours(10));

			var projection = target.CreateProjection();

			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc)), projection.Period());
			Assert.AreEqual(new TimeSpan(10, 0, 0), projection.ContractTime());
		}

		[Test]
		public void OnlyAbsenceWithinPeriodAffectContractTime()
		{
			setup();
			var pAbs = createPersonAbsence(100, new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			var projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.IsNull(projection.Period());
			Assert.AreEqual(new TimeSpan(0), projection.ContractTime());
		}

		[Test]
		public void VerifyLongAbsenceContractTimeInPlus5Time()
		{
			setup();
			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(12));

			var pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			var projection = target.CreateProjection();
			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new TimeSpan(12, 0, 0), projection.ContractTime());
		}

		[Test]
		public void VerifyLongAbsenceContractTimeInMinus5Time()
		{
			setup();
			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime =
				new WorkTime(TimeSpan.FromHours(12));
			var pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			var projection = target.CreateProjection();
			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new TimeSpan(12, 0, 0), projection.ContractTime());

			var fakeLayer = projection.First();
			Assert.AreSame(target.FakeActivity, ((VisualLayer)fakeLayer).HighestPriorityActivity);
			Assert.AreSame(pAbs.Layer.Payload, fakeLayer.Payload);
			Assert.AreEqual("Fake activity", target.FakeActivity.Description.Name);
		}

		[Test]
		public void VerifyOnlyFakeLayersFullyCoveredByAbsenceIsCreated()
		{
			setup();
			var pAbs = createPersonAbsence(100, createPeriod(10, 21));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(12));

			var projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
		}

		[Test]
		public void NoFakeLayerIfNoPersonPeriod()
		{
			setup();
			var pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			scheduleDay.Add(pAbs);

			CollectionAssert.IsEmpty(target.CreateProjection());
		}

		[Test]
		public void VerifyDayOffOnNonShiftDay()
		{
			setup();
			scheduleDay.Person.PermissionInformation.SetDefaultTimeZone(
				(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			var dOff = new DayOffTemplate(new Description("test"))
			{
				Anchor = TimeSpan.FromHours(12)
			};
			dOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(1));
			scheduleDay.PersonAssignment(true).SetDayOff(dOff);

			var projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.AreEqual(TimeSpan.Zero, projection.ContractTime());
			scheduleDay.Add(createPersonAbsence(100, createPeriod(2, 22)));
			projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.AreEqual(TimeSpan.Zero, projection.ContractTime());
		}

	    [Test]
	    public void VerifyContractTimeIsZeroOnEmptyDayWithEmptyContract()
	    {
			setup();
			addPeriodAndContractToPerson(false);
	        scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime =
	            new WorkTime(TimeSpan.Zero);

	        scheduleDay.Add(createPersonAbsence(100, createPeriod(-10, 20)));

	        var projection = target.CreateProjection();
	        Assert.AreEqual(0, projection.Count());
	    }

	    [Test]
		public void VerifyContractTimeIsZeroIfOvertime()
		{
			setup();
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);

			Assert.AreEqual(TimeSpan.Zero, ass.ProjectionService().CreateProjection().ContractTime());
		}

		[Test]
		public void VerifyContractTimeIsZeroIfOvertimeOverlappedWithContractAbsence()
		{
			setup();
			var def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);
			scheduleDay.Add(ass);

			var abs = createPersonAbsence(100, createPeriod(0, 24));
			abs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(abs);

			var res = target.CreateProjection();
			Assert.AreEqual(TimeSpan.Zero, res.ContractTime());
			Assert.AreEqual(createPeriod(10, 12), res.Period());
			Assert.IsInstanceOf<IAbsence>(new List<IVisualLayer>(res)[0].Payload);
		}

		[Test]
		public void OvertimeShouldBeZeroIfAbsence()
		{
			setup();
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var personContract = new PersonContract(new Contract("d"), new PartTimePercentage("d"), new ContractSchedule("d"));
			personContract.Contract.AddMultiplicatorDefinitionSetCollection(set);
			scheduleDay.Person.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), personContract, new Team()));
			var pa = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			pa.AddOvertimeActivity(new Activity("d"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), set);

			var abs = new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(new Absence(), period));

			scheduleDay.Add(pa);
			scheduleDay.Add(abs);

			scheduleDay.ProjectionService().CreateProjection().Overtime()
				.Should().Be.EqualTo(TimeSpan.Zero);
		}

		[Test]
		public void VerifyContractTimeAndCorrectActivityIfOvertimeOverlappedWithMeeting()
		{
			setup();
			var def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);
			scheduleDay.Add(ass);
			var meeting = createPersonMeeting(createPeriod(10, 12));
			scheduleDay.Add(meeting);

			var res = target.CreateProjection();
			Assert.AreEqual(TimeSpan.Zero, res.ContractTime());
			Assert.AreEqual(createPeriod(10, 12), res.Period());
			Assert.AreEqual(meeting.BelongsToMeeting.Activity.Description,
				new List<IVisualLayer>(res)[0].Payload.ConfidentialDescription(null));
		}

		[Test]
		public void ContractTimeShouldBeCalculatedCorrectOnEachLayerIfInContractTimeIsMixed()
		{
			setup();
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			var actCtr = new Activity("phone");
			var actNoCtr = new Activity("lunch") { InContractTime = false };
			ass.AddActivity(actCtr, createPeriod(8, 17));
			ass.AddActivity(actNoCtr, createPeriod(11, 12));
			var absLayer = new AbsenceLayer(new Absence { InContractTime = true }, createPeriod(10, 20));
			var personAbs = new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, absLayer);
			scheduleDay.Add(ass);
			scheduleDay.Add(personAbs);

			var proj = target.CreateProjection();

			Assert.AreEqual(2, proj.Count());
			Assert.AreEqual(TimeSpan.FromHours(8), proj.ContractTime());
			Assert.AreEqual(TimeSpan.FromHours(2), proj.FilterLayers(proj.First().Period).ContractTime());
			Assert.AreEqual(TimeSpan.FromHours(6), proj.FilterLayers(proj.Last().Period).ContractTime());
		}

		[Test]
		public void VerifyAbsencesOverlappingEndOfNightshiftWhenFakeLayer()
		{
			setup();
			const int nextDay = 24;
			var scheduleRange = new ScheduleRange(scheduleDay.Owner,
				new ScheduleParameters(scheduleDay.Scenario, scheduleDay.Person, scheduleDay.Owner.Period.VisiblePeriod), permissionChecker, new FullPermission());

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Person,
				scheduleDay.Scenario, createPeriod(20, nextDay + 12));
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
				createPeriod(nextDay + 3, nextDay + 50));
			scheduleRange.Add(abs);
			scheduleRange.Add(ass);
			var fakeLayerLength = addPeriodAndContractToPerson(true);

			var proj1 =
				new List<IVisualLayer>(scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1)).ProjectionService().CreateProjection());
			var proj2 =
				new List<IVisualLayer>(scheduleRange.ScheduledDay(new DateOnly(2000, 1, 2)).ProjectionService().CreateProjection());

			Assert.AreEqual(2, proj1.Count);
			Assert.AreEqual(createPeriod(20, nextDay + 3), proj1[0].Period);
			Assert.AreEqual(createPeriod(nextDay + 3, nextDay + 12), proj1[1].Period);

			Assert.AreEqual(1, proj2.Count);
			Assert.AreEqual(createPeriod(nextDay + 7, nextDay + 7 + fakeLayerLength.Hours), proj2[0].Period);
		}

		[Test]
		public void FakeLayerShouldBeCreatedIfDayOffAndAbsenceAndContract()
		{
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Person,
				scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(true);
			var proj = target.CreateProjection();
			proj.Should().Have.Count.EqualTo(1);
		}

        [Test]
        public void FakeLayerShouldHaveRightContractTimeForFullDayAbsenceIfContractTimeIsFromContract()
        {
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
		        createPeriod(-100, 100));
            abs.Layer.Payload.InContractTime = true;
            scheduleDay.Add(abs);
            var schedWeek = new ContractScheduleWeek();
            var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999, 1, 1));
            scheduleDay.Person.AddPersonPeriod(period);
	        period.PersonContract.PartTimePercentage = new PartTimePercentage("half")
	        {
		        Percentage = new Percent(0.5)
	        };
            period.PersonContract.Contract.WorkTimeSource = WorkTimeSource.FromContract;
            period.PersonContract.ContractSchedule.AddContractScheduleWeek(schedWeek);
            schedWeek.Add(DayOfWeek.Monday, true);
            schedWeek.Add(DayOfWeek.Tuesday, true);
            schedWeek.Add(DayOfWeek.Wednesday, true);
            schedWeek.Add(DayOfWeek.Thursday, true);
            schedWeek.Add(DayOfWeek.Friday, true);
            schedWeek.Add(DayOfWeek.Saturday, true);
            schedWeek.Add(DayOfWeek.Sunday, true);

            var proj = target.CreateProjection();
            Assert.That(proj.Count(), Is.EqualTo(1));
            Assert.That(proj.First().Period.ElapsedTime(), Is.EqualTo(TimeSpan.FromHours(4)));
        }
        
        [Test]
        public void FakeLayerShouldHaveRightContractTimeForFullDayAbsenceIfContractTimeIsFromSchedulePeriod()
        {
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
		        createPeriod(-100, 100));
            abs.Layer.Payload.InContractTime = true;
            scheduleDay.Add(abs);
            var schedWeek = new ContractScheduleWeek();
            var dateOnly = new DateOnly(1999, 1, 1);
            var period = PersonPeriodFactory.CreatePersonPeriod(dateOnly);
            scheduleDay.Person.AddPersonPeriod(period);
            period.PersonContract.PartTimePercentage = new PartTimePercentage("half") {Percentage = new Percent(0.5)};
            period.PersonContract.Contract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            period.PersonContract.ContractSchedule.AddContractScheduleWeek(schedWeek);
            schedWeek.Add(DayOfWeek.Monday, true);
            schedWeek.Add(DayOfWeek.Tuesday, true);
            schedWeek.Add(DayOfWeek.Wednesday, true);
            schedWeek.Add(DayOfWeek.Thursday, true);
            schedWeek.Add(DayOfWeek.Friday, true);
            schedWeek.Add(DayOfWeek.Saturday, true);
            schedWeek.Add(DayOfWeek.Sunday, true);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
            schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
            scheduleDay.Person.AddSchedulePeriod(schedulePeriod);
            var proj = target.CreateProjection();
            Assert.That(proj.Count(), Is.EqualTo(1));
            Assert.That(proj.First().Period.ElapsedTime(), Is.EqualTo(TimeSpan.FromHours(6)));
        }

	    [Test]
		public void FakeLayerShouldBeCreatedIfDayOffAndAbsenceAndNoContract()
	    {
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
			    createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
		    var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Person,
			    scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(false);
			var proj = target.CreateProjection();
			proj.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void NoContractTimeIfFakeLayerIsCreatedDueToDayOffAndAbsenceAndContract()
		{
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Person,
				scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(true);
			var proj = target.CreateProjection();
			proj.ContractTime().TotalMinutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void NoContractTimeIfFakeLayerIsCreatedDueToDayOffAndAbsenceAndNoContract()
		{
			setup();
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Person,
				scheduleDay.Scenario, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(false);
			var proj = target.CreateProjection();
			proj.ContractTime().TotalMinutes.Should().Be.EqualTo(0);
		}

        [Test]
        public void ShouldGetContractTimeFromSchedulePeriodForProjection()
        {
			setup();
			var dateOnly = new DateOnly(2012, 12, 1);
            var person = PersonFactory.CreatePerson();
            var team = TeamFactory.CreateSimpleTeam("Team");
	        var personContract = new PersonContract(new Contract("contract")
	        {
		        WorkTimeSource = WorkTimeSource.FromSchedulePeriod
	        }, new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
            var personPeriod = new PersonPeriod(dateOnly, personContract, team);
            person.AddPersonPeriod(personPeriod);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
            schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
            person.AddSchedulePeriod(schedulePeriod);

            var scheduleday = ExtractedSchedule.CreateScheduleDay(dic, person, dateOnly, new FullPermission());
            target = new ScheduleProjectionService(scheduleday, new ProjectionPayloadMerger());
	        var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scheduleday.Scenario,
		        new DateTimePeriod(new DateTime(2012, 11, 29, 0, 0, 0, DateTimeKind.Utc),
			        new DateTime(2012, 12, 2, 0, 0, 0, DateTimeKind.Utc)));
            abs.Layer.Payload.InContractTime = true;
            scheduleday.Add(abs);
            var proj = target.CreateProjection();

            proj.ContractTime().TotalHours.Should().Be.EqualTo(6);
        }

		private static DateTimePeriod createPeriod(int startHour, int endHour)
		{
			var date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var start = date.AddHours(startHour);
			var end = date.AddHours(endHour);
			return new DateTimePeriod(start, end);
		}
	}
}
