using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ScheduleProjectionServiceTest
	{
		private IScheduleDay scheduleDay;
		private ScheduleProjectionService target;
	    private ScheduleDictionary dic;

	    [SetUp]
		public void Setup()
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			dic = new ScheduleDictionary(new Scenario("sd"), new ScheduleDateTimePeriod(new DateTimePeriod(1900, 1, 1, 2200, 1, 1)));
			scheduleDay = ExtractedSchedule.CreateScheduleDay(dic, person, new DateOnly(2000, 1, 1));
			target = new ScheduleProjectionService(scheduleDay, new ProjectionPayloadMerger());
		}

		[Test]
		public void VerifyFakeActivity()
		{
			IActivity fake = target.FakeActivity;
			Assert.AreSame(target.FakeActivity, fake);
			Assert.IsTrue(fake.InWorkTime);
			Assert.IsTrue(fake.InPaidTime);
			Assert.IsTrue(fake.InContractTime);
		}


		private IPersonAbsence createPersonAbsence(byte prioAndName, DateTimePeriod period)
		{
			var prioAsString = prioAndName.ToString(CultureInfo.CurrentCulture);
			IAbsence abs = AbsenceFactory.CreateAbsence(prioAsString);
			abs.Description = new Description(prioAsString);
			abs.Priority = prioAndName;
			return new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(abs, period));
		}

		[Test]
		public void VerifyEnumeratorDoesNotMessWithLayers()
		{
			IPersonAssignment ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario, scheduleDay.Person, createPeriod(8, 16));
			IActivity pActivity = ActivityFactory.CreateActivity("personal");
			ass1.AddPersonalActivity(pActivity, createPeriod(10,13));
			IPersonAbsence abs1 = createPersonAbsence(100, createPeriod(8, 13));
			scheduleDay.Add(ass1);
			scheduleDay.Add(abs1);

			IVisualLayerCollection visualCollection = target.CreateProjection();

			IVisualLayerCollection anotherVisualCollection = visualCollection.FilterLayers(createPeriod(11, 12));
			Assert.AreEqual(1, anotherVisualCollection.Count());
			visualCollection.GetEnumerator();
			anotherVisualCollection = visualCollection.FilterLayers(createPeriod(11, 12));
			Assert.AreEqual(1, anotherVisualCollection.Count());

		}

		[Test]
		public void VerifyMultiplePersonAbsencesAndCorrectUnderlyingLayers()
		{
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("första"), createPeriod(2, 9));
			ass.AddActivity(new Activity("andra"), createPeriod(11, 19));
			IPersonAbsence abs1 = createPersonAbsence(70, createPeriod(-1, 2)); //no
			IPersonAbsence abs2 = createPersonAbsence(60, createPeriod(-50, 3));
			IPersonAbsence abs3 = createPersonAbsence(50, createPeriod(10, 12)); //no
			IPersonAbsence abs4 = createPersonAbsence(40, createPeriod(10, 12)); 
			IPersonAbsence abs5 = createPersonAbsence(30, createPeriod(9, 11));
			IPersonAbsence abs6 = createPersonAbsence(20, createPeriod(19, 50)); //no
			IPersonAbsence abs7 = createPersonAbsence(10, createPeriod(8, 12));
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
			VisualLayer layer = (VisualLayer)resWrapper[0];
			var actLayer1 = ass.MainLayers().First();
			var actLayer2 = ass.MainLayers().Last();

			Assert.AreEqual("60", layer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(createPeriod(2, 3), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[1];
			Assert.AreEqual("första", layer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(createPeriod(3, 8), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.IsNull(layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[2];
			Assert.AreEqual("10", layer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(createPeriod(8, 9), layer.Period);
			Assert.AreSame(actLayer1.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[3];
			Assert.AreEqual("10", layer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(createPeriod(11, 12), layer.Period);
			Assert.AreSame(actLayer2.Payload, layer.HighestPriorityActivity);
			Assert.AreSame(layer.Payload, layer.HighestPriorityAbsence);

			layer = (VisualLayer)resWrapper[4];
			Assert.AreEqual("andra", layer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(createPeriod(12, 19), layer.Period);
			Assert.AreSame(actLayer2.Payload, layer.HighestPriorityActivity);
			Assert.IsNull(layer.HighestPriorityAbsence);
		}

		[Test]
		public void VerifyTotallyOverlapping()
		{
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario,
																	scheduleDay.Person,
																	new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			IPersonAbsence abs = createPersonAbsence(100, new DateTimePeriod(1900, 1, 1, 2010, 1, 1));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			VisualLayer retLayer = (VisualLayer)resWrapper[0];
			Assert.AreEqual(1, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), retLayer.Period);
			Assert.AreEqual("100", retLayer.Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreSame(ass.MainLayers().First().Payload, retLayer.HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, retLayer.HighestPriorityAbsence);
		}


		[Test]
		public void VerifyResultWhenAbsenceAndContractSaysDayOff()
		{
			scheduleDay.Add(PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(0, 24)));

			addPeriodAndContractToPerson(false);

			var avgWorkTimePerDay = scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.AverageWorkTimePerDay;
			//verify setup
			Assert.IsFalse(scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.ContractSchedule.IsWorkday(new DateOnly(2000, 1, 1), new DateOnly(2000, 1, 1)));
			Assert.IsTrue(avgWorkTimePerDay.Ticks > 0);

			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(1, resWrapper.Count);
			Assert.IsFalse(resWrapper[0].Payload.InContractTime);
			Assert.IsTrue(resWrapper[0].Payload is IAbsence);
			Assert.AreEqual(resWrapper[0].Period.ElapsedTime(), avgWorkTimePerDay);
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
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario, scheduleDay.Person,
													 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			IPersonAbsence abs = createPersonAbsence(100, new DateTimePeriod(1900, 1, 1, 2000, 6, 1));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(2, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 6, 1), resWrapper[0].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 1, 2001, 1, 1), resWrapper[1].Period);
			Assert.AreEqual("100", resWrapper[0].Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(ass.MainLayers().First().Payload.Description, resWrapper[1].Payload.ConfidentialDescription(null,DateOnly.Today));
			Assert.AreSame(ass.MainLayers().First().Payload, ((VisualLayer)resWrapper[0]).HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)resWrapper[0]).HighestPriorityAbsence);
			Assert.AreSame(ass.MainLayers().First().Payload, ((VisualLayer)resWrapper[1]).HighestPriorityActivity);
			Assert.IsNull(((VisualLayer)resWrapper[1]).HighestPriorityAbsence);
		}

		[Test]
		public void VerifyTinyOverlapping()
		{
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario, scheduleDay.Person,
													 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));

			IPersonAbsence abs = createPersonAbsence(100, new DateTimePeriod(2000, 6, 1, 2000, 6, 2));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			var resWrapper = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(3, resWrapper.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 6, 1), resWrapper[0].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 1, 2000, 6, 2), resWrapper[1].Period);
			Assert.AreEqual(new DateTimePeriod(2000, 6, 2, 2001, 1, 1), resWrapper[2].Period);
			Assert.AreEqual(ass.MainLayers().First().Payload.Description, resWrapper[0].Payload.ConfidentialDescription(null,DateOnly.Today));
			Assert.AreEqual("100", resWrapper[1].Payload.ConfidentialDescription(null,DateOnly.Today).Name);
			Assert.AreEqual(ass.MainLayers().First().Payload.Description, resWrapper[2].Payload.ConfidentialDescription(null, DateOnly.Today));

			Assert.AreSame(ass.MainLayers().First().Payload, resWrapper[0].Payload);
			Assert.AreSame(abs.Layer.Payload, resWrapper[1].Payload);
			Assert.AreSame(ass.MainLayers().First().Payload, resWrapper[2].Payload);
		}

		[Test]
		public void VerifyOverlappingTwoAssignmentsWithDifferentActivities()
		{
			var ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(ActivityFactory.CreateActivity("1"), new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			ass.AddActivity(ActivityFactory.CreateActivity("2"), new DateTimePeriod(2001, 1, 1, 2002, 1, 1));

			IPersonAbsence abs = createPersonAbsence(100, new DateTimePeriod(1900, 6, 1, 2100, 6, 2));
			scheduleDay.Add(ass);
			scheduleDay.Add(abs);
			var res = new List<IVisualLayer>(target.CreateProjection());
			Assert.AreEqual(1, res.Count);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2002, 1, 1), res[0].Period);
			Assert.AreEqual(abs.Layer.Payload, res[0].Payload);
			Assert.AreSame(ass.MainLayers().First().Payload, ((VisualLayer)res[0]).HighestPriorityActivity);
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)res[0]).HighestPriorityAbsence);
		}

		[Test]
		public void VerifyAssignmentAbsenceMeeting()
		{
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("1"), createPeriod(0, 1));
			ass.AddActivity(new Activity("2"), createPeriod(1, 20));

			IPersonMeeting meeting = CreatePersonMeeting(createPeriod(2, 6));
			IPersonAbsence abs = createPersonAbsence(100, createPeriod(8, 20));

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
			IPersonAbsence abs = createPersonAbsence(100, createPeriod(10, 20));
			IPersonMeeting meeting = CreatePersonMeeting(createPeriod(13, 14));

			scheduleDay.Add(abs);
			scheduleDay.Add(meeting);
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario, scheduleDay.Person,
																			 createPeriod(10, 20)));

			var res = target.CreateProjection();
			Assert.AreEqual(1, res.Count());
			Assert.AreSame(abs.Layer.Payload, ((VisualLayer)res.First()).Payload);
			Assert.AreEqual(createPeriod(10, 20), res.First().Period);
		}

		[Test]
		public void VerifyMergeDoesNotMergeDifferentUnderlyingActivityInContractTime()
		{
			Activity phone = new Activity("phone") { InContractTime = true };
			Activity lunch = new Activity("lunch") { InContractTime = false };
			Absence sjuk = new Absence { InContractTime = true };


			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(phone, new DateTimePeriod(2000, 1, 1, 2000, 1, 4));
			ass.AddActivity(lunch, new DateTimePeriod(2000, 1, 2, 2000, 1, 3));

			IPersonAbsence abs = new PersonAbsence(ass.Person, ass.Scenario,
													new AbsenceLayer(sjuk, new DateTimePeriod(2000, 1, 1, 2003, 1, 1)));
			scheduleDay.Add(ass);
			Assert.AreEqual(new TimeSpan(2, 0, 0, 0), target.CreateProjection().ContractTime());
			scheduleDay.Add(abs);
			IVisualLayerCollection res = target.CreateProjection();
			Assert.AreEqual(new TimeSpan(2, 0, 0, 0), res.ContractTime());
			Assert.AreEqual(1, res.Count());
			Assert.AreSame(sjuk, new List<IVisualLayer>(res)[0].Payload);
		}

		private IPersonMeeting CreatePersonMeeting(DateTimePeriod period)
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
			DateTimePeriod perYes = new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc),
														new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc));
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithPersonalShift(ActivityFactory.CreateActivity("f"),
																				scheduleDay.Person,
																				new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
																				scheduleDay.Scenario));

			IPersonAbsence pAbs = createPersonAbsence(100, perYes);
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(10));


			var projection = target.CreateProjection();


			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new DateTimePeriod(new DateTime(2000, 1, 1, 7, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 17, 0, 0, DateTimeKind.Utc)), projection.Period());
			Assert.AreEqual(new TimeSpan(0), projection.ReadyTime());
			Assert.AreEqual(new TimeSpan(10, 0, 0), projection.ContractTime());
		}

		[Test]
		public void OnlyAbsenceWithinPeriodAffectContractTime()
		{
			IPersonAbsence pAbs = createPersonAbsence(100, new DateTimePeriod(2000, 1, 3, 2000, 1, 4));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			IVisualLayerCollection projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.IsNull(projection.Period());
			Assert.AreEqual(new TimeSpan(0), projection.ReadyTime());
			Assert.AreEqual(new TimeSpan(0), projection.ContractTime());
		}

		[Test]
		public void VerifyLongAbsenceContractTimeInPlus5Time()
		{
			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(12));

			IPersonAbsence pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);
			IVisualLayerCollection projection = target.CreateProjection();
			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new TimeSpan(12, 0, 0), projection.ContractTime());
		}

		[Test]
		public void VerifyLongAbsenceContractTimeInMinus5Time()
		{
			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(12));
			IPersonAbsence pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);
			IVisualLayerCollection projection = target.CreateProjection();
			Assert.AreEqual(1, projection.Count());
			Assert.AreEqual(new TimeSpan(12, 0, 0), projection.ContractTime());
			IVisualLayer fakeLayer = projection.First();
			Assert.AreSame(target.FakeActivity, ((VisualLayer)fakeLayer).HighestPriorityActivity);
			Assert.AreSame(pAbs.Layer.Payload, fakeLayer.Payload);
			Assert.AreEqual("Fake activity", target.FakeActivity.Description.Name);
		}

		[Test]
		public void VerifyOnlyFakeLayersFullyCoveredByAbsenceIsCreated()
		{
			IPersonAbsence pAbs = createPersonAbsence(100, createPeriod(10, 21));
			pAbs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(pAbs);

			addPeriodAndContractToPerson(true);
			scheduleDay.Person.Period(new DateOnly(2000, 1, 1)).PersonContract.Contract.WorkTime = new WorkTime(TimeSpan.FromHours(12));

			IVisualLayerCollection projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
		}

		[Test]
		public void NoFakeLayerIfNoPersonPeriod()
		{
			IPersonAbsence pAbs = createPersonAbsence(100, new DateTimePeriod(1999, 12, 31, 2000, 1, 20));
			scheduleDay.Add(pAbs);

			CollectionAssert.IsEmpty(target.CreateProjection());
		}

		[Test]
		public void VerifyDayOffOnNonShiftDay()
		{
			scheduleDay.Person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			DayOffTemplate dOff = new DayOffTemplate(new Description("test"));
			dOff.Anchor = TimeSpan.FromHours(12);
			dOff.SetTargetAndFlexibility(TimeSpan.FromHours(24), TimeSpan.FromHours(1));
			scheduleDay.PersonAssignment(true).SetDayOff(dOff);

			IVisualLayerCollection projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.AreEqual(TimeSpan.Zero, projection.ContractTime());
			Assert.AreEqual(TimeSpan.Zero, projection.ReadyTime());
			scheduleDay.Add(createPersonAbsence(100, createPeriod(2, 22)));
			projection = target.CreateProjection();
			Assert.AreEqual(0, projection.Count());
			Assert.AreEqual(TimeSpan.Zero, projection.ContractTime());
			Assert.AreEqual(TimeSpan.Zero, projection.ReadyTime());
		}

	    [Test]
	    public void VerifyContractTimeIsZeroOnEmptyDayWithEmptyContract()
	    {
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
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);

			Assert.AreEqual(TimeSpan.Zero, ass.ProjectionService().CreateProjection().ContractTime());
		}

		[Test]
		public void VerifyContractTimeIsZeroIfOvertimeOverlappedWithContractAbsence()
		{
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);
			scheduleDay.Add(ass);
			IPersonAbsence abs = createPersonAbsence(100, createPeriod(0, 24));
			abs.Layer.Payload.InContractTime = true;
			scheduleDay.Add(abs);

			IVisualLayerCollection res = target.CreateProjection();
			Assert.AreEqual(TimeSpan.Zero, res.ContractTime());
			Assert.AreEqual(createPeriod(10, 12), res.Period());
			Assert.IsInstanceOf<IAbsence>(new List<IVisualLayer>(res)[0].Payload);
		}

		[Test]
		public void OvertimeShouldBeZeroIfAbsence()
		{
			var period = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var set = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("overtime", MultiplicatorType.Overtime);
			var personContract = new PersonContract(new Contract("d"),
																 new PartTimePercentage("d"),
																 new ContractSchedule("d"));
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
			IMultiplicatorDefinitionSet def = new MultiplicatorDefinitionSet("foo", MultiplicatorType.Overtime);
			PersonFactory.AddDefinitionSetToPerson(scheduleDay.Person, def);
			IPersonAssignment ass = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000, 1, 1));
			ass.AddOvertimeActivity(new Activity("d"), createPeriod(10, 12), def);
			scheduleDay.Add(ass);
			IPersonMeeting meeting = CreatePersonMeeting(createPeriod(10, 12));
			scheduleDay.Add(meeting);

			IVisualLayerCollection res = target.CreateProjection();
			Assert.AreEqual(TimeSpan.Zero, res.ContractTime());
			Assert.AreEqual(createPeriod(10, 12), res.Period());
			Assert.AreEqual(meeting.BelongsToMeeting.Activity.Description, new List<IVisualLayer>(res)[0].Payload.ConfidentialDescription(null,DateOnly.Today));
		}

		[Test]
		public void ContractTimeShouldBeCalculatedCorrectOnEachLayerIfInContractTimeIsMixed()
		{
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
			const int nextDay = 24;
			var scheduleRange = new ScheduleRange(scheduleDay.Owner,
												  new ScheduleParameters(scheduleDay.Scenario, scheduleDay.Person,
																		 scheduleDay.Owner.Period.VisiblePeriod));

			var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(scheduleDay.Scenario, scheduleDay.Person, createPeriod(20, nextDay + 12));
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(nextDay + 3, nextDay + 50));
			scheduleRange.Add(abs);
			scheduleRange.Add(ass);
			var fakeLayerLength = addPeriodAndContractToPerson(true);

			var proj1 = new List<IVisualLayer>(scheduleRange.ScheduledDay(new DateOnly(2000, 1, 1)).ProjectionService().CreateProjection());
			var proj2 = new List<IVisualLayer>(scheduleRange.ScheduledDay(new DateOnly(2000, 1, 2)).ProjectionService().CreateProjection());

			Assert.AreEqual(2, proj1.Count());
			Assert.AreEqual(createPeriod(20, nextDay + 3), proj1[0].Period);
			Assert.AreEqual(createPeriod(nextDay + 3, nextDay + 12), proj1[1].Period);

			Assert.AreEqual(1, proj2.Count());
			Assert.AreEqual(createPeriod(nextDay + 7, nextDay + 7 + fakeLayerLength.Hours), proj2[0].Period);
		}

		[Test]
		public void FakeLayerShouldBeCreatedIfDayOffAndAbsenceAndContract()
		{
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Scenario, scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(true);
			var proj = target.CreateProjection();
			proj.Should().Have.Count.EqualTo(1);
		}

        [Test]
        public void FakeLayerShouldHaveRightContractTimeForFullDayAbsenceIfContractTimeIsFromContract()
        {
            var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario,
                                                               createPeriod(-100, 100));
            abs.Layer.Payload.InContractTime = true;
            scheduleDay.Add(abs);
            var schedWeek = new ContractScheduleWeek();
            var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999, 1, 1));
            scheduleDay.Person.AddPersonPeriod(period);
            period.PersonContract.PartTimePercentage = new PartTimePercentage("half") {Percentage = new Percent(0.5)};
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
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Scenario, scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(false);
			var proj = target.CreateProjection();
			proj.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void NoContractTimeIfFakeLayerIsCreatedDueToDayOffAndAbsenceAndContract()
		{
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Scenario, scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(true);
			var proj = target.CreateProjection();
			proj.ContractTime().TotalMinutes.Should().Be.EqualTo(0);
		}

		[Test]
		public void NoContractTimeIfFakeLayerIsCreatedDueToDayOffAndAbsenceAndNoContract()
		{
			var abs = PersonAbsenceFactory.CreatePersonAbsence(scheduleDay.Person, scheduleDay.Scenario, createPeriod(-100, 100));
			abs.Layer.Payload.InContractTime = true;
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Scenario, scheduleDay.Person, scheduleDay.DateOnlyAsPeriod.DateOnly, new DayOffTemplate(new Description("sdf")));
			scheduleDay.Add(abs);
			scheduleDay.Add(dayOff);
			addPeriodAndContractToPerson(false);
			var proj = target.CreateProjection();
			proj.ContractTime().TotalMinutes.Should().Be.EqualTo(0);
		}

        [Test]
        public void ShouldGetContractTimeFromSchedulePeriodForProjection()
        {
            var dateOnly = new DateOnly(2012, 12, 1);
            var person = PersonFactory.CreatePerson();
            var team = TeamFactory.CreateSimpleTeam("Team");
            var personContract = new PersonContract(new Contract("contract") { WorkTimeSource = WorkTimeSource.FromSchedulePeriod},
                    new PartTimePercentage("Testing"), new ContractSchedule("Test1"));
            var personPeriod = new PersonPeriod(dateOnly, personContract, team);
            person.AddPersonPeriod(personPeriod);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(dateOnly);
            schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
            person.AddSchedulePeriod(schedulePeriod);

            var scheduleday = ExtractedSchedule.CreateScheduleDay(dic, person, dateOnly);
            target = new ScheduleProjectionService(scheduleday, new ProjectionPayloadMerger());
            var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scheduleday.Scenario,
                                                               new DateTimePeriod(
                                                                   new DateTime(2012, 11, 29, 0, 0, 0, DateTimeKind.Utc),
                                                                   new DateTime(2012, 12, 2, 0, 0, 0, DateTimeKind.Utc)));
            abs.Layer.Payload.InContractTime = true;
            scheduleday.Add(abs);
            var proj = target.CreateProjection();

            proj.ContractTime().TotalHours.Should().Be.EqualTo(6);
        }

		private static DateTimePeriod createPeriod(int startHour, int endHour)
		{
			DateTime date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime start = date.AddHours(startHour);
			DateTime end = date.AddHours(endHour);
			return new DateTimePeriod(start, end);
		}
	}
}
