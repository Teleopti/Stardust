using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

using List = Rhino.Mocks.Constraints.List;

namespace Teleopti.Ccc.DomainTest.Collection
{
	[DomainTest]
	public class ScheduleDictionaryNoMockTest
	{
		private readonly IScheduleDateTimePeriod _period = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
		
		[Test]
		public void CanModifyExistingPermittedData()
		{
			var authorizer = new FakePermissions();

			using (CurrentAuthorization.ThreadlyUse(authorizer))
			{
				authorizer.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifySchedule);
				authorizer.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules);

				var scenario = ScenarioFactory.CreateScenarioAggregate();
				var person = PersonFactory.CreatePerson();
				var noNewRules = NewBusinessRuleCollection.Minimum();
				var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
				
				var target = new ScheduleDictionary(scenario, _period,
					new DifferenceEntityCollectionService<IPersistableScheduleData>(),
					new ByPassPersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), authorizer);
				var part = target[person].ScheduledDay(new DateOnly(2000, 6, 1));
				var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();
				var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();

				var schedule = (Schedule)part;
				schedule.Add(ass);
				schedule.Add(abs);
				var errors = target.Modify(ScheduleModifier.Scheduler, part, noNewRules, scheduleDayChangeCallback,
					new ScheduleTagSetter(NullScheduleTag.Instance)).ToList();

				errors.Count.Should().Be.EqualTo(0);
				target.DifferenceSinceSnapshot().Count().Should().Be.EqualTo(2);
			}
		}

		[Test]
		public void CanModifyExistingPermittedDataThroughByPassingDefaultPermission()
		{
			var authorizer = new FakePermissions();

			using (CurrentAuthorization.ThreadlyUse(authorizer))
			{
				authorizer.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifySchedule);
				authorizer.HasPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules);

				var scenario = ScenarioFactory.CreateScenarioAggregate();
				var person = PersonFactory.CreatePerson();
				var noNewRules = NewBusinessRuleCollection.Minimum();
				var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
				var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();
				var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();
				
				var target = new ScheduleDictionary(scenario, _period,
					new DifferenceEntityCollectionService<IPersistableScheduleData>(),
					new ByPassPersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), authorizer);
				var part = target[person].ScheduledDay(new DateOnly(2000, 6, 1));
				var schedule = (Schedule)part;
				schedule.Add(ass);
				schedule.Add(abs);
				var errors = target.Modify(ScheduleModifier.Scheduler, part, noNewRules, scheduleDayChangeCallback,
					new ScheduleTagSetter(NullScheduleTag.Instance)).ToList();

				errors.Count.Should().Be.EqualTo(0);
				target.DifferenceSinceSnapshot().Count().Should().Be.EqualTo(2);
				target.AnyDifferenceSinceSnapshotInVisiblePeriod().Should().Be.True();
			}
		}

		[Test]
		public void CanNotModifyExistingDataThroughDefaultPermissionCheckerWhenNoPermisson()
		{
			var authorizer = new FakePermissions();
			
			using (CurrentAuthorization.ThreadlyUse(authorizer))
			{
				authorizer.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewSchedules);
				var scenario = ScenarioFactory.CreateScenarioAggregate();
				var person = PersonFactory.CreatePerson();
				var noNewRules = NewBusinessRuleCollection.Minimum();
				var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
				
				var target = new ScheduleDictionary(scenario, _period,
					new DifferenceEntityCollectionService<IPersistableScheduleData>(),
					new ByPassPersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), authorizer);
				var part = target[person].ScheduledDay(new DateOnly(2000, 6, 1));
				var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();
				var abs = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2000, 6, 1, 2000, 6, 2)).WithId();

				Schedule schedule = (Schedule)part;
				schedule.Add(ass);
				schedule.Add(abs);
				
				target.UsePermissions(false);
				target.Modify(ScheduleModifier.Scheduler, part, noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance)).ToList();
				target.DifferenceSinceSnapshot().Count().Should().Be.EqualTo(0);
				target.AnyDifferenceSinceSnapshotInVisiblePeriod().Should().Be.False();
			}
		}
	}

	[DomainTest]
	public class ScheduleDictionaryTest
	{
		private IContract _contract;
		private IScheduleRange _dummyScheduleRange;
		private TimeSpan _nightlyRest;
		private IDictionary<IPerson, IScheduleRange> dictionary;
		private IDifferenceCollectionService<IPersistableScheduleData> diffSvc;
		private string dummyFunction;
		private IPerson dummyPerson;
		private bool eventFired;
		private MockRepository mocks;
		private ScheduleDateTimePeriod period;
		private IScenario scenario;
		private ScheduleDictionary target;
		private INewBusinessRuleCollection _noNewRules;
		private IAuthorization authorization;
		private IScheduleDayChangeCallback scheduleDayChangeCallback;
		private IPersistableScheduleDataPermissionChecker dataPermissionChecker;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "justToCreateTheScheduleRangeForTests"), SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			dictionary = mocks.StrictMock<IDictionary<IPerson, IScheduleRange>>();
			diffSvc = mocks.StrictMock<IDifferenceCollectionService<IPersistableScheduleData>>();
			authorization = mocks.StrictMock<IAuthorization>();
			scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>();
			period = new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			scenario = ScenarioFactory.CreateScenarioAggregate();
			dataPermissionChecker = new PersistableScheduleDataPermissionChecker(new ThisAuthorization(authorization));
			target = new ScheduleDictionary(scenario, period, new DifferenceEntityCollectionService<IPersistableScheduleData>(), dataPermissionChecker, new ThisAuthorization(authorization));
			dummyPerson = PersonFactory.CreatePerson();
			IScheduleRange justToCreateTheScheduleRangeForTests = target[dummyPerson];
			dummyFunction = DefinedRaptorApplicationFunctionPaths.ViewSchedules;
			_dummyScheduleRange =
				new ScheduleRange(target, new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), dummyPerson, new DateTimePeriod(2000, 1, 1, 2001, 1, 1)), dataPermissionChecker, new ThisAuthorization(authorization));

			_noNewRules = NewBusinessRuleCollection.Minimum();

			_contract = ContractFactory.CreateContract("for test");
			_nightlyRest = new TimeSpan(8, 0, 0);
			_contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, new TimeSpan(40, 0, 0),
									   _nightlyRest,
									   new TimeSpan(50, 0, 0));
			ITeam team = TeamFactory.CreateSimpleTeam();
			dummyPerson.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1900, 1, 1),
											   PersonContractFactory.CreatePersonContract(
												_contract,
												PartTimePercentageFactory.CreatePartTimePercentage("sdf"),
												ContractScheduleFactory.CreateContractSchedule("sdf")),
												team));
			target.PartModified += target_PartModified;
			eventFired = false;
		}
		
		[Test]
		public void VerifyAddKeyValuePairNotSupported()
		{
			var item =
				new KeyValuePair<IPerson, IScheduleRange>(dummyPerson,
				new ScheduleRange(target, new ScheduleParameters(scenario, dummyPerson, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)), dataPermissionChecker, new ThisAuthorization(authorization)));
			Assert.Throws<NotSupportedException>(() => target.Add(item));
		}

		[Test]
		public void VerifyPermissionCheckIsTurnedOnByDefault()
		{
			Assert.IsTrue(target.PermissionsEnabled);
		}

		[Test]
		public void VerifyCanGetAllSchedulesForPeriod()
		{
			IPerson dummyPerson1 = PersonFactory.CreatePerson();
			IPerson dummyPerson2 = PersonFactory.CreatePerson();
			IPersonAssignment ass =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
										  scenario, ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), ShiftCategoryFactory.CreateShiftCategory("sdf"));
			IPersonAssignment ass1 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
										  scenario, ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4), ShiftCategoryFactory.CreateShiftCategory("sdf"));
			IPersonAssignment ass2 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson1,
										  scenario, ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2), ShiftCategoryFactory.CreateShiftCategory("sdf"));
			IPersonAssignment ass3 =
				PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson2,
										  scenario, ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 3, 2000, 1, 4), ShiftCategoryFactory.CreateShiftCategory("sdf"));

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					((ScheduleRange)target[dummyPerson]).Add(ass);
					((ScheduleRange)target[dummyPerson]).Add(ass1);
					((ScheduleRange)target[dummyPerson1]).Add(ass2);
					((ScheduleRange)target[dummyPerson2]).Add(ass3);
					Assert.AreEqual(3, target.Count);

					var schedList = target.SchedulesForDay(new DateOnly(2000, 1, 1));
					int withOneAss = 0, withZeroAss = 0;
					foreach (IScheduleDay part in schedList)
					{
						if (part.PersonAssignment() == null)
							withZeroAss++;
						else
							withOneAss++;
					}

					Assert.AreEqual(3, schedList.Count());
					Assert.AreEqual(1, withZeroAss);
					Assert.AreEqual(2, withOneAss);
				}
			}
		}

		private void SetAuthorizationServiceExpectations()
		{
			DateOnlyPeriod dop = period.VisiblePeriod.ToDateOnlyPeriod(dummyPerson.PermissionInformation.DefaultTimeZone());
			Expect.Call(authorization.PermittedPeriods(string.Empty, new DateOnlyPeriod(), null))
				.IgnoreArguments()
				.Repeat.Any()
				.Return(new List<DateOnlyPeriod> { dop });
			Expect.Call(authorization.IsPermitted("Any"))
				.IgnoreArguments()
				.Return(true)
				.Repeat.Any();
			Expect.Call(authorization.IsPermitted("", new DateOnly(), dummyPerson))
				.IgnoreArguments()
				.Return(true)
				.Repeat.Any();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyClone()
		{

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					//first put something in
					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															  scenario, new DateTimePeriod(
							2000, 6,
							1,
							2000, 6,
							2));
					IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario,
													  new DateTimePeriod(2000, 6, 1, 2000, 6,
															 2));


					Guid assId = Guid.NewGuid();
					Guid absId = Guid.NewGuid();
					ass.SetId(assId);
					abs.SetId(absId);
					Schedule schedule = (Schedule)part;
					schedule.Add(ass);
					schedule.Add(abs);

					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					ScheduleDictionary targetClone = (ScheduleDictionary)target.Clone();
					Assert.IsNotNull(targetClone);
				}
			}
		}

		[Test]
		public void ShouldCallDayChangedCallbackBeforeAndAfterScheduleChange()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(() => scheduleDayChangeCallback.ScheduleDayBeforeChanging()).IgnoreArguments();
				Expect.Call(() => scheduleDayChangeCallback.ScheduleDayChanged(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					//first put something in
					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															  scenario, new DateTimePeriod(
							2000, 6,
							1,
							2000, 6,
							2));
					IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario,
													  new DateTimePeriod(2000, 6, 1, 2000, 6,
															 2));

					Guid assId = Guid.NewGuid();
					Guid absId = Guid.NewGuid();
					ass.SetId(assId);
					abs.SetId(absId);
					Schedule schedule = (Schedule)part;
					schedule.Add(ass);
					schedule.Add(abs);

					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					ScheduleDictionary targetClone = (ScheduleDictionary)target.Clone();
					Assert.IsNotNull(targetClone);
				}
			}
		}

		[Test]
		public void VerifyCorrectParametersToDifferenceSinceSnapshot()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new FullPermission());
			IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(2000, 11, 2, 2001, 1, 1));
			pAss.SetId(Guid.NewGuid());

			IDayOffTemplate dOff1 = DayOffFactory.CreateDayOff(new Description("test"));
			dOff1.Anchor = TimeSpan.Zero;
			dOff1.SetTargetAndFlexibility(TimeSpan.FromHours(3), TimeSpan.FromHours(1));
			IPersonAbsence pAbs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss });
			target.TakeSnapshot();
			((ScheduleRange)target[dummyPerson]).Add(pAbs);

			//this is actually "wrong" but do this for easier mocking-asserts
			pAbs.SetId(Guid.NewGuid());

			using (mocks.Record())
			{
				Expect.Call(diffSvc.Difference(null, null))
					.IgnoreArguments()
					.Constraints(
						List.Equal(new List<IPersistableScheduleData> { pAss }),
						List.ContainsAll(new List<IPersistableScheduleData> { pAss, pAbs })
						)
					.Return(new DifferenceCollection<IPersistableScheduleData>());
			}
			using (mocks.Playback())
			{

				target.DifferenceSinceSnapshot();
			}
		}

		[Test]
		public void VerifyDeleteDoNotDoAnythingIfIdDoesNotExists()
		{
			Assert.IsNull(target.DeleteFromBroker(Guid.NewGuid()));
			Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
		}

		[Test]
		public void VerifyDeleteFromDataSource()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					//first put something in
					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															  scenario, new DateTimePeriod(
							2000, 6,
							1,
							2000, 6,
							2));
					IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario,
													  new DateTimePeriod(2000, 6, 1, 2000, 6,
															 2));

					IDayOffTemplate dOff1 = DayOffFactory.CreateDayOff(new Description("test"));
					dOff1.Anchor = TimeSpan.FromHours(10);
					dOff1.SetTargetAndFlexibility(TimeSpan.FromHours(4), TimeSpan.FromHours(1));
					Guid assId = Guid.NewGuid();
					Guid absId = Guid.NewGuid();
					ass.SetId(assId);
					abs.SetId(absId);
					Schedule schedule = (Schedule)part;
					schedule.Add(ass);
					schedule.Add(abs);

					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
					target.TakeSnapshot();

					Assert.AreSame(ass, target.DeleteFromBroker(assId));
					Assert.AreSame(abs, target.DeleteFromBroker(absId));
					Assert.IsFalse(target[dummyPerson].Contains(ass));
					Assert.IsFalse(target[dummyPerson].Contains(abs));

					var diff = target.DifferenceSinceSnapshot();
					Assert.AreEqual(0, diff.Count());

					Assert.IsTrue(eventFired);
				}
			}
		}

		[Test]
		public void VerifyDeleteMeetingFromDataSource()
		{
			var date = new DateOnly(2000, 6, 1);
			Guid assId = Guid.NewGuid();
			Guid meetingId = Guid.NewGuid();

			var meetingLoader = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();

			SetAuthorizationServiceExpectations();

			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
													 scenario, new DateTimePeriod(2000, 6, 1,
					2000, 6, 2));
			ass.SetId(assId);

			IMeetingPerson meetingPerson = new MeetingPerson(dummyPerson, false);

			IMeeting meeting = new Meeting(dummyPerson, new List<IMeetingPerson>(), "subject", "location", "description", ActivityFactory.CreateActivity("act"), target.Scenario);
			meeting.StartDate = date;
			meeting.EndDate = meeting.StartDate;
			meeting.StartTime = TimeSpan.FromHours(11);
			meeting.EndTime = meeting.StartTime.Add(TimeSpan.FromMinutes(30));
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(meetingId);


			Expect.Call(meetingLoader.LoadAggregate(meetingId)).Return(meeting);

			mocks.ReplayAll();

			using (CurrentAuthorization.ThreadlyUse(authorization))
			{
				//first put something in
				IScheduleDay part = target[dummyPerson].ScheduledDay(date);

				part.Add(ass);

				target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				target.MeetingUpdateFromBroker(meetingLoader, meetingId);
				target.TakeSnapshot();

				var part2 = target[dummyPerson].ScheduledDay(date);
				Assert.AreEqual(1, part2.PersonMeetingCollection().Length);
				target.DeleteMeetingFromBroker(meetingId);
				var partNew = target[dummyPerson].ScheduledDay(date);
				Assert.AreEqual(0, partNew.PersonMeetingCollection().Length);
				var diff = target.DifferenceSinceSnapshot();
				Assert.AreEqual(0, diff.Count());

				Assert.IsTrue(eventFired);
			}
			mocks.VerifyAll();
		}

		[Test]
		public void VerifyDeleteFromDataSourceReturnNullIfNotFound()
		{
			Assert.IsNull(target.DeleteFromBroker(Guid.NewGuid()));
		}

		[Test]
		public void VerifyDifferenceSinceSnapshotDoesNotCrashIfNoSnapshot()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					//this will happen on scheduleranges not initialized when loading scheduledictionary
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod(
							2000,
							11, 2,
							2001,
							1, 1));
					IScheduleRange range = target[dummyPerson];
					IScheduleDay part = range.ScheduledDay(new DateOnly(2000, 11, 2));
					((Schedule)part).Add(pAss);
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					var diff = target.DifferenceSinceSnapshot();
					Assert.AreEqual(1, diff.Count());
					Assert.AreEqual(DifferenceStatus.Added, diff.First().Status);
				}
			}
		}

		[Test]
		public void VerifyModifyCallsPersonAssignmentCleanup()
		{
			var part = mocks.DynamicMock<IScheduleDay>();

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(part.Person).Return(dummyPerson).Repeat.AtLeastOnce();
				Expect.Call(part.PersonAbsenceCollection()).Return(new IPersonAbsence[0]).Repeat.AtLeastOnce();
				Expect.Call(part.PersistableScheduleDataCollection()).Return(new List<IPersistableScheduleData>()).Repeat.AtLeastOnce();
				Expect.Call(part.PersonAssignment()).Return(null);
				Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2000, 1, 1), dummyPerson.PermissionInformation.DefaultTimeZone()));
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourcePersonWithNoPermission()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new ThisAuthorization(authorization));
			target.PartModified += target_PartModified;
			var dummy = target[dummyPerson];

			IPersonAssignmentRepository rep = mocks.StrictMock<IPersonAssignmentRepository>();
			Guid newId = Guid.NewGuid();

			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			ass.SetId(newId);
			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(ass);
				Expect.Call(authorization.PermittedPeriods(string.Empty, new DateOnlyPeriod(), null))
					.IgnoreArguments()
					.Repeat.Any()
					.Return(new List<DateOnlyPeriod>());
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.AreSame(ass, target.UpdateFromBroker(rep, newId));
				}
			}
			Assert.IsFalse(target[dummyPerson].Contains(ass));
			Assert.IsTrue(target[dummyPerson].Contains(ass, true));
			Assert.IsTrue(eventFired);
		}

		[Test]
		public void VerifyMeetingUpdateFromDataSourcePersonWithNoPermission()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new ThisAuthorization(authorization));
			target.PartModified += target_PartModified;
			var personAtMeeting = PersonFactory.CreatePerson();
			var dummy = target[dummyPerson];
			dummy = target[personAtMeeting];

			var rep = mocks.StrictMock<IMeetingRepository>();
			Guid newId = Guid.NewGuid();
			IMeeting meeting = CreateMeeting(personAtMeeting);

			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(meeting);
				Expect.Call(authorization.PermittedPeriods(string.Empty, new DateOnlyPeriod(), null))
					.IgnoreArguments()
					.Repeat.Any()
					.Return(new List<DateOnlyPeriod>());
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.MeetingUpdateFromBroker(rep, newId);
				}
			}
			Assert.IsTrue(eventFired);
		}

		private IMeeting CreateMeeting(IPerson personAtMeeting)
		{
			IMeeting meeting = new Meeting(dummyPerson, new List<IMeetingPerson> { new MeetingPerson(personAtMeeting, true) }, string.Empty, string.Empty,
							   string.Empty, ActivityFactory.CreateActivity("d"), scenario);
			meeting.StartDate = new DateOnly(2000, 1, 2);
			meeting.EndDate = new DateOnly(2000, 1, 3);
			return meeting;
		}

		[Test]
		public void VerifyDifferenceSinceSnapshotReturnsNothing()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod(
							2000,
							11, 2,
							2001,
							1, 1));

					IPersonAbsence pAbs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario,
													   new DateTimePeriod(2000, 1, 1, 2001,
															  1, 1));
					pAss.SetId(Guid.NewGuid());
					pAbs.SetId(Guid.NewGuid());
					((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss });
					((ScheduleRange)target[dummyPerson]).Add(pAbs);
					target.TakeSnapshot();

					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
				}
			}
		}

		[Test]
		public void VerifyExtractAllScheduleData()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IPersonAssignment pAss =
						PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(),
												  scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
					pAss.SetId(Guid.NewGuid());
					IPersonAbsence pAbs =
						PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(), scenario,
											 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
					pAbs.SetId(Guid.NewGuid());
					extractor extractor = new extractor();
					((ScheduleRange)target[pAss.Person]).Add(pAss);
					((ScheduleRange)target[pAbs.Person]).Add(pAbs);
					target.ExtractAllScheduleData(extractor, new DateTimePeriod(1999, 12, 30, 2000, 1, 3));
					CollectionAssert.Contains(extractor.Data, pAss);
					CollectionAssert.Contains(extractor.Data, pAbs);
				}
			}
		}

		[Test]
		public void VerifyExtractAllScheduleDataWithPeriod()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IPersonAssignment pAss =
						PersonAssignmentFactory.CreateAssignmentWithMainShift(PersonFactory.CreatePerson(),
												  scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
					pAss.SetId(Guid.NewGuid());

					IPersonAbsence pAbs =
						PersonAbsenceFactory.CreatePersonAbsence(PersonFactory.CreatePerson(), scenario,
											 new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
					pAbs.SetId(Guid.NewGuid());

					extractor extractor = new extractor();
					((ScheduleRange)target[pAss.Person]).Add(pAss);
					((ScheduleRange)target[pAbs.Person]).Add(pAbs);
					target.ExtractAllScheduleData(extractor, new DateTimePeriod(2000, 1, 1, 2000, 1, 3));
					CollectionAssert.Contains(extractor.Data, pAss);
					CollectionAssert.Contains(extractor.Data, pAbs);
				}
			}
		}

		[Test]
		public void VerifySchedulePeriodWhenPublishingDateAndPreferenceDateDoesNotCorrespond()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new ThisAuthorization(authorization));
			IPerson person = PersonFactory.CreatePerson();
			ITeam team = TeamFactory.CreateSimpleTeam("MyTeam");
			IWorkflowControlSet workflowControlSet = new WorkflowControlSet("d");
			workflowControlSet.SchedulePublishedToDate = new DateTime(2000, 1, 10);
			workflowControlSet.PreferencePeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(2000, 2, 10, 2000, 2, 11);
			person.WorkflowControlSet = workflowControlSet;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			IPersonPeriod personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(1999, 1, 1), PersonContractFactory.CreatePersonContract(), team);
			person.AddPersonPeriod(personPeriod);
			IPersonAssignment pAss1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
										  scenario, new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2).ToDateTimePeriod(timeZone));//Expected
			pAss1.SetId(Guid.NewGuid());

			IPersonAssignment pAss2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
										  scenario, new DateOnlyPeriod(2000, 2, 1, 2000, 2, 2).ToDateTimePeriod(timeZone));

			using (mocks.Record())
			{
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
					.Return(false).Repeat.Any();
				var dateOnlyPeriod = new DateOnlyPeriod(2000,1,1,2000,12,31);
				Expect.Call(authorization.PermittedPeriods(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
						dateOnlyPeriod, person))
					.Return(new []{dateOnlyPeriod});
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					((ScheduleRange)target[pAss1.Person]).Add(pAss1);
					((ScheduleRange)target[pAss2.Person]).Add(pAss2);
					CollectionAssert.Contains(
						target[person].ScheduledDay(new DateOnly(2000, 1, 1)).PersistableScheduleDataCollection(), pAss1);
					CollectionAssert.DoesNotContain(
						target[person].ScheduledDay(new DateOnly(2000, 2, 1)).PersistableScheduleDataCollection(), pAss2);
				}
			}
		}

		[Test]
		public void VerifyFunctionPermission()
		{
			//what should happen if no permission on function? Right now - does nothing
			target = new ScheduleDictionary(scenario, period, diffSvc, new PersistableScheduleDataPermissionChecker(new ThisAuthorization(authorization)), new ThisAuthorization(authorization));

			IDayOffTemplate dOff1 = DayOffFactory.CreateDayOff(new Description("test"));
			dOff1.Anchor = TimeSpan.Zero;
			dOff1.SetTargetAndFlexibility(TimeSpan.FromHours(3), TimeSpan.FromHours(1));
			IPersonAbsence pAbs2BeChanged = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			IPersonAssignment pAss2BeAdded = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(2000, 1, 1, 2001, 1, 1));
			const string function = DefinedRaptorApplicationFunctionPaths.ViewSchedules;

			using (mocks.Record())
			{
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
					.Return(true).Repeat.Any();
				var dop = period.VisiblePeriod.ToDateOnlyPeriod(dummyPerson.PermissionInformation.DefaultTimeZone());
				Expect.Call(authorization.PermittedPeriods(function, dop, dummyPerson))
					.Return(new List<DateOnlyPeriod> { dop })
					.Repeat.AtLeastOnce();
				Expect.Call(authorization.IsPermitted("", new DateOnly(), dummyPerson)).IgnoreArguments().Return(false).Repeat
					.Any();
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)).
					Return(false);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					((ScheduleRange)target[dummyPerson]).Add(pAbs2BeChanged);

					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 1, 1));
					var personAbsence = part.PersonAbsenceCollection()[0];
					var oldLayer = personAbsence.Layer;
					var newLayer = new AbsenceLayer(oldLayer.Payload, oldLayer.Period.MovePeriod(TimeSpan.FromDays(10)));
					part.Remove(personAbsence);
					var newPersonAbsence = new PersonAbsence(personAbsence.Person, personAbsence.Scenario, newLayer);
					part.Add(newPersonAbsence);
					part.Add(pAss2BeAdded);

					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
					//nothing should have been changed here!
					var res = target[dummyPerson].ScheduledDay(new DateOnly(2000, 1, 1));

					res.PersonAssignment().Should().Be.Null();
					Assert.AreEqual(1, res.PersonAbsenceCollection().Length);
					Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1),
							res.PersonAbsenceCollection()[0].Layer.Period);
				}
			}
		}

		[Test]
		public void VerifyItemAddsNewIfNotExists()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IPersonAssignment ass =
						PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
						scenario, ActivityFactory.CreateActivity("sdf"), new DateTimePeriod(2000, 1, 1, 2001, 1, 1), ShiftCategoryFactory.CreateShiftCategory("sdf"));
					((ScheduleRange)target[dummyPerson]).Add(ass);
					Assert.AreEqual(1, target.Count);
					Assert.IsTrue(target[dummyPerson].Contains(ass));
					Assert.AreEqual(1, target.Count);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyWriteProtection()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new ThisAuthorization(authorization));
			IPerson per = PersonFactory.CreatePerson();
			using (mocks.Record())
			{
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
					.Return(true);
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection))
					.Return(true);
				var dop = period.VisiblePeriod.ToDateOnlyPeriod(per.PermissionInformation.DefaultTimeZone());
				Expect.Call(authorization.PermittedPeriods(dummyFunction, dop, per))
					.Return(new List<DateOnlyPeriod> { dop });
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
					.Return(false);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					per.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(2001, 1, 1);
					var part = target[per].ScheduledDay(new DateOnly(2000, 1, 2));
					part.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(per,
													   scenario, new DateTimePeriod(2000, 1, 2, 2000,
							1, 3)));
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
				}
			}
		}

		[Test]
		public void ShouldNotModifyWhenNoRightsToModifyCurrentScenario()
		{
			target = new ScheduleDictionary(scenario, period, diffSvc, dataPermissionChecker, new ThisAuthorization(authorization));
			var per = PersonFactory.CreatePerson();
			var dop = period.VisiblePeriod.ToDateOnlyPeriod(per.PermissionInformation.DefaultTimeZone());

			using (mocks.Record())
			{
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)).Return(true).Repeat.Twice();
				Expect.Call(authorization.PermittedPeriods(dummyFunction, dop, per)).Return(new List<DateOnlyPeriod> { dop });
				Expect.Call(authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario)).Return(false);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					var part = target[per].ScheduledDay(new DateOnly(2000, 1, 2));
					scenario.Restricted = true;
					part.Add(PersonAssignmentFactory.CreateAssignmentWithMainShift(per,
													   scenario, new DateTimePeriod(2000, 1, 2, 2000,
							1, 3)));
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					Assert.AreEqual(0,
							target[per].ScheduledDay(new DateOnly(2000, 1, 2)).PersistableScheduleDataCollection
								().Count());
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourcePersonAbsence()
		{
			eventFired = false;

			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IPersonAbsence>>();
			Guid newId = Guid.NewGuid();

			IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			abs.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(abs);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.AreSame(abs, target.UpdateFromBroker(rep, newId));
					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					Assert.AreEqual(1, part.PersistableScheduleDataCollection().OfType<IPersonAbsence>().Count());
					Assert.AreEqual(abs.Id, part.PersistableScheduleDataCollection().OfType<IPersonAbsence>().First().Id);
					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
					Assert.IsTrue(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourcePreferenceDay()
		{
			eventFired = false;

			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IPreferenceDay>>();
			Guid newId = Guid.NewGuid();

			IPreferenceDay preferenceDay = new PreferenceDay(dummyPerson, new DateOnly(2000, 1, 3), new PreferenceRestriction());
			preferenceDay.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(preferenceDay);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.AreSame(preferenceDay, target.UpdateFromBroker(rep, newId));
					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 1, 3));
					Assert.AreEqual(1, part.PersonRestrictionCollection().Length);
					Assert.AreEqual(preferenceDay.Id, ((IPreferenceDay)part.PersonRestrictionCollection()[0]).Id);
					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
					Assert.IsTrue(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourcePersonAbsenceWithWrongScenario()
		{
			eventFired = false;

			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IPersonAbsence>>();
			Guid newId = Guid.NewGuid();

			IScenario theOtherScenario = ScenarioFactory.CreateScenarioAggregate();
			IPersonAbsence abs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, theOtherScenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			abs.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(abs);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.IsNull(target.UpdateFromBroker(rep, newId));
					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					Assert.AreEqual(0, part.PersistableScheduleDataCollection().OfType<IPersonAbsence>().Count());
					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
					Assert.IsFalse(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourceDoNotDoAnythingIfDataIsNull()
		{
			eventFired = false;

			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IPersonAbsence>>();
			Guid newId = Guid.NewGuid();

			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(null);
			}
			using (mocks.Playback())
			{
				Assert.IsNull(target.UpdateFromBroker(rep, newId));
			}

			Assert.IsFalse(eventFired);
		}

		[Test]
		public void VerifyUpdateFromDataSourceOutsideRangeDoesNotDoAnything()
		{
			IPersonAssignmentRepository rep = mocks.StrictMock<IPersonAssignmentRepository>();
			Guid newId = Guid.NewGuid();

			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(1900, 6, 1, 1900, 6, 2));
			ass.SetId(newId);

			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(ass);
			}
			using (mocks.Playback())
			{
				Assert.IsNull(target.UpdateFromBroker(rep, newId));
			}
			Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
			Assert.IsFalse(eventFired);
		}

		[Test]
		public void VerifyUpdateOnlyOccursIfRangeAlreadyIsCreated()
		{
			target.Remove(dummyPerson);

			IPersonAssignmentRepository rep = mocks.StrictMock<IPersonAssignmentRepository>();
			Guid newId = Guid.NewGuid();

			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			ass.SetId(newId);

			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(ass);
			}
			using (mocks.Playback())
			{
				Assert.IsNull(target.UpdateFromBroker(rep, newId));
			}
			Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
			Assert.IsFalse(eventFired);
		}

		[Test]
		public void VerifyUpdateFromDataSourcePersonAssignment()
		{
			IPersonAssignmentRepository rep = mocks.StrictMock<IPersonAssignmentRepository>();
			Guid newId = Guid.NewGuid();

			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, scenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			ass.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(ass);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.AreSame(ass, target.UpdateFromBroker(rep, newId));
					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					part.PersonAssignment().Should().Not.Be.Null();
					Assert.AreEqual(ass.Id, part.PersonAssignment().Id);
					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
					Assert.IsTrue(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourcePersonAssignmentWithWrongScenario()
		{
			eventFired = false;

			IPersonAssignmentRepository rep = mocks.StrictMock<IPersonAssignmentRepository>();
			Guid newId = Guid.NewGuid();

			IScenario theOtherScenario = ScenarioFactory.CreateScenarioAggregate();
			IPersonAssignment ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson, theOtherScenario, new DateTimePeriod(2000, 6, 1, 2001, 1, 1));
			ass.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(ass);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					Assert.IsNull(target.UpdateFromBroker(rep, newId));
					var part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 6, 1));
					part.PersonAssignment().Should().Be.Null();
					Assert.AreEqual(0, target.DifferenceSinceSnapshot().Count());
					Assert.IsFalse(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourceMeeting()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			var newId = Guid.NewGuid();

			var dateOnlyPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			IMeetingPerson meetingPerson = new MeetingPerson(target.Keys.First(), false);

			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", ActivityFactory.CreateActivity("act"), target.Scenario);
			meeting.StartDate = dateOnlyPeriod.StartDate;
			meeting.EndDate = dateOnlyPeriod.EndDate;
			meeting.StartTime = TimeSpan.FromHours(11);
			meeting.EndTime = meeting.StartTime.Add(TimeSpan.FromMinutes(30));
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId)).Return(meeting);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.MeetingUpdateFromBroker(rep, newId);
					var part = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 1));
					var part2 = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 2));
					Assert.AreEqual(1, part.PersonMeetingCollection().Length);
					Assert.AreEqual(1, part2.PersonMeetingCollection().Length);
					Assert.IsTrue(eventFired);
				}
			}
		}

		[Test]
		public void ShouldNotUpdateMeetingOnPersonsNotInDictionary()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			var newId = Guid.NewGuid();

			var dateOnlyPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			IMeetingPerson meetingPerson = new MeetingPerson(PersonFactory.CreatePerson("person"), false);

			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", ActivityFactory.CreateActivity("act"), target.Scenario)
			{
				StartDate = dateOnlyPeriod.StartDate,
				EndDate = dateOnlyPeriod.EndDate,
				StartTime = TimeSpan.FromHours(11)
			};

			meeting.EndTime = meeting.StartTime.Add(TimeSpan.FromMinutes(30));
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(newId);

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId)).Return(meeting);
			}

			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.MeetingUpdateFromBroker(rep, newId);
					var part = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 1));
					var part2 = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 2));
					Assert.AreEqual(0, part.PersonMeetingCollection().Length);
					Assert.AreEqual(0, part2.PersonMeetingCollection().Length);
					Assert.IsFalse(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourceMeetingOutsideValidPeriod()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			Guid newId = Guid.NewGuid();

			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			IMeetingPerson meetingPerson = new MeetingPerson(PersonFactory.CreatePerson(), false);

			eventFired = false;
			IMeeting meeting =
				new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", ActivityFactory.CreateActivity("act"), target.Scenario); //outside period
			meeting.StartDate = dateOnlyPeriod.StartDate.AddDays(369);
			meeting.EndDate = meeting.StartDate;
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(meeting);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.MeetingUpdateFromBroker(rep, newId);
					var part = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 1));
					Assert.AreEqual(0, part.PersonMeetingCollection().Length);
					Assert.IsFalse(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateFromDataSourceMeetingForWrongScenario()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			Guid newId = Guid.NewGuid();

			IScenario theOtherScenario = ScenarioFactory.CreateScenarioAggregate();
			IMeetingPerson meetingPerson = new MeetingPerson(PersonFactory.CreatePerson(), false);

			DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject",
							   "location", "description", ActivityFactory.CreateActivity("act"),
							   theOtherScenario);
			meeting.StartDate = dateOnlyPeriod.StartDate;
			meeting.EndDate = dateOnlyPeriod.EndDate;
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(newId);

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId))
					.Return(meeting);
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					target.MeetingUpdateFromBroker(rep, newId);
					var part = target[meetingPerson.Person].ScheduledDay(new DateOnly(2000, 1, 1));
					Assert.AreEqual(0, part.PersonMeetingCollection().Length);
					Assert.IsFalse(eventFired);
				}
			}
		}

		[Test]
		public void VerifyUpdateMeetingWhenNull()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			Guid newId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(rep.LoadAggregate(newId))
					.Return(null);
			}
			using (mocks.Playback())
			{
				target.MeetingUpdateFromBroker(rep, newId);
			}
			Assert.IsFalse(eventFired);
		}

		[Test]
		public void VerifyNoEventUpdateFromDataSourceMeetingIfPersonNotInOrganization()
		{
			var rep = mocks.StrictMock<ILoadAggregateFromBroker<IMeeting>>();
			var newId = Guid.NewGuid();

			var dateOnlyPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			IMeetingPerson meetingPerson = new MeetingPerson(new Person().WithName(new Name("uffe", "uffe")), false);

			IMeeting meeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description", ActivityFactory.CreateActivity("act"), target.Scenario);
			meeting.StartDate = dateOnlyPeriod.StartDate;
			meeting.EndDate = dateOnlyPeriod.EndDate;
			meeting.StartTime = TimeSpan.FromHours(11);
			meeting.EndTime = meeting.StartTime.Add(TimeSpan.FromMinutes(30));
			meeting.AddMeetingPerson(meetingPerson);
			meeting.SetId(newId);
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
				Expect.Call(rep.LoadAggregate(newId)).Return(meeting);
			}
			using (mocks.Playback())
			{
				target.MeetingUpdateFromBroker(rep, newId);
				Assert.IsFalse(eventFired);
			}
		}

		private void target_PartModified(object sender, ModifyEventArgs e)
		{
			eventFired = true;
		}

		private class extractor : IScheduleExtractor
		{
			private readonly ConcurrentBag<IScheduleData> _data = new ConcurrentBag<IScheduleData>();

			public IList<IScheduleData> Data
			{
				get { return _data.ToArray(); }
			}

			public void AddSchedulePart(IScheduleDay schedulePart)
			{
				schedulePart.PersistableScheduleDataCollection().ForEach(_data.Add);
			}
		}


		#region UndoRedo
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyUndoRedo()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IUndoRedoContainer container = new UndoRedoContainer();
					target.SetUndoRedoContainer(container);
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod(
							2000,
							11, 2,
							2001,
							1, 1));

					IPersonAbsence pAbs = PersonAbsenceFactory.CreatePersonAbsence(dummyPerson, scenario,
													   new DateTimePeriod(2000, 1, 1, 2001,
															  1, 1));
					((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss });
					((ScheduleRange)target[dummyPerson]).Add(pAbs);


					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2));
					part.Clear<IPersonAssignment>();
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).
								PersonAssignment().Should().Be.Null();

					container.Undo();
					target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).
					PersonAssignment().Should().Not.Be.Null();

					container.Redo();
					target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).
					PersonAssignment().Should().Be.Null();
				}
			}
		}


		[Test]
		public void VerifyCanUndoCanRedo()
		{

			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IUndoRedoContainer container = new UndoRedoContainer();
					target.SetUndoRedoContainer(container);
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod(
							2000, 11, 1, 2000,
							11, 2));
					IPersonAssignment pAss2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															scenario, new DateTimePeriod(
							2000, 11, 2,
							2000, 11, 3));
					((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss, pAss2 });
					Assert.IsFalse(container.CanUndo());
					Assert.IsFalse(container.CanRedo());

					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 1));
					part.PersonAssignment().ClearMainActivities();

					container.CreateBatch("a");
					target.Modify(ScheduleModifier.Scheduler, new List<IScheduleDay> { part }, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
					container.CommitBatch();

					Assert.IsFalse(container.CanRedo());
					Assert.IsTrue(container.CanUndo());


					target.PartModified += PartModified_VerifyFromUndoRedo;
					container.Undo();
					target.PartModified -= PartModified_VerifyFromUndoRedo;

					Assert.IsTrue(container.CanRedo());
					Assert.IsFalse(container.CanUndo());

				}
			}
		}

		private static void PartModified_VerifyFromUndoRedo(object sender, ModifyEventArgs e)
		{
			Assert.AreEqual(ScheduleModifier.UndoRedo, e.Modifier);
		}

		[Test]
		public void VerifyNotPossibleUndoRedoDoesNothing()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IUndoRedoContainer container = new UndoRedoContainer();
					target.SetUndoRedoContainer(container);
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod(
							2000,
							11, 2,
							2001,
							1, 1));
					((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss });

					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2));
					part.PersonAssignment().ClearMainActivities();
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
					CollectionAssert.IsEmpty(target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).PersonAssignment().MainActivities());

					container.Undo();
					Assert.IsNotNull(target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).PersonAssignment().ShiftCategory);

					//should do nothing
					container.Undo();
					Assert.IsNotNull(target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).PersonAssignment().ShiftCategory);

					container.Redo();
					CollectionAssert.IsEmpty(target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).PersonAssignment().MainActivities());

					//should do nothing
					container.Redo();
					CollectionAssert.IsEmpty(target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2)).PersonAssignment().MainActivities());
				}
			}
		}

		[Test]
		public void WhenModifyingTheRedoStackShouldBeCleared()
		{
			using (mocks.Record())
			{
				SetAuthorizationServiceExpectations();
			}
			using (mocks.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(authorization))
				{
					IUndoRedoContainer container = new UndoRedoContainer();
					target.SetUndoRedoContainer(container);
					IPersonAssignment pAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(dummyPerson,
															   scenario, new DateTimePeriod
						(2000, 11,
							2, 2001,
							1, 1));
					((ScheduleRange)target[dummyPerson]).AddRange(new List<IPersonAssignment> { pAss });

					IScheduleDay part = target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2));
					var oldMainShift = part.GetEditorShift();
					part.PersonAssignment().ClearMainActivities();
					target.Modify(ScheduleModifier.Scheduler, part, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					container.Undo();

					Assert.IsTrue(container.CanRedo());

					IScheduleDay part2 = target[dummyPerson].ScheduledDay(new DateOnly(2000, 11, 2));
					part.AddMainShift(oldMainShift);
					target.Modify(ScheduleModifier.Scheduler, part2, _noNewRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));

					Assert.IsFalse(container.CanRedo());
				}
			}
		}
		#endregion

		#region IDictionary tests
		[Test]
		public void VerifyContainsKey()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				Expect.Call(dictionary.ContainsKey(dummyPerson))
					.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.ContainsKey(dummyPerson));
			}
		}

		[Test]
		public void VerifyTryGetValue()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				Expect.Call(dictionary.TryGetValue(dummyPerson, out _dummyScheduleRange))
					.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.TryGetValue(dummyPerson, out _dummyScheduleRange));
			}
		}

		[Test]
		public void VerifyItem()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				Expect.Call(dictionary[dummyPerson])
					.Return(_dummyScheduleRange);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(_dummyScheduleRange, dictionary[dummyPerson]);
			}
		}


		[Test]
		public void VerifySet()
		{
			Assert.Throws<NotSupportedException>(() => target[dummyPerson] = new ScheduleRange(target, new ScheduleParameters(scenario, dummyPerson, new DateTimePeriod(2001, 1, 1, 2002, 1, 1)), dataPermissionChecker, new ThisAuthorization(authorization)));
		}


		[Test]
		public void VerifyAddNoSupported()
		{
			Assert.Throws<NotSupportedException>(() => target.Add(dummyPerson, _dummyScheduleRange));
		}


		[Test]
		public void VerifyAddKeyValueNotSupported()
		{
			Assert.Throws<NotSupportedException>(() => target.Add(new KeyValuePair<IPerson, IScheduleRange>(dummyPerson, _dummyScheduleRange)));
		}

		[Test]
		public void VerifyKeys()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			ICollection<IPerson> keys = new List<IPerson>();
			using (mocks.Record())
			{
				Expect.Call(dictionary.Keys).Return(keys);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(keys, target.Keys);
			}
		}

		[Test]
		public void VerifyValues()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			ICollection<IScheduleRange> values = new List<IScheduleRange>();
			using (mocks.Record())
			{
				Expect.Call(dictionary.Values).Return(values);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(values, target.Values);
			}
		}
		[Test]
		public void VerifyClear()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				dictionary.Clear();
			}
			using (mocks.Playback())
			{
				target.Clear();
			}
		}
		[Test]
		public void VerifyContains()
		{

			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			KeyValuePair<IPerson, IScheduleRange> key = new KeyValuePair<IPerson, IScheduleRange>(dummyPerson, _dummyScheduleRange);
			using (mocks.Record())
			{
				Expect.Call(dictionary.Contains(key))
					.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsTrue(target.Contains(key));
			}
		}
		[Test]
		public void VerifyCopyTo()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			KeyValuePair<IPerson, IScheduleRange>[] array = new KeyValuePair<IPerson, IScheduleRange>[0];
			using (mocks.Record())
			{
				dictionary.CopyTo(array, 17);
			}
			using (mocks.Playback())
			{
				target.CopyTo(array, 17);
			}
		}
		[Test]
		public void VerifyCount()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				Expect.Call(dictionary.Count)
					.Return(12);
			}
			using (mocks.Playback())
			{
				Assert.AreEqual(12, target.Count);
			}
		}

		[Test]
		public void VerifyIsReadOnly()
		{
			Assert.IsFalse(target.IsReadOnly);
		}
		[Test]
		public void VerifyEnumerator()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			IEnumerator<KeyValuePair<IPerson, IScheduleRange>> enumerator =
				mocks.StrictMock<IEnumerator<KeyValuePair<IPerson, IScheduleRange>>>();
			using (mocks.Record())
			{
				Expect.Call(dictionary.GetEnumerator())
					.Return(enumerator);
			}
			using (mocks.Playback())
			{
				Assert.AreSame(enumerator, target.GetEnumerator());
			}
		}


		[Test]
		public void VerifyRemove()
		{
			target = new ScheduleDictionaryForTest(scenario, period, dictionary);
			using (mocks.Record())
			{
				Expect.Call(dictionary.Remove(dummyPerson))
					.Return(false);
				Expect.Call(dictionary.Remove(new KeyValuePair<IPerson, IScheduleRange>(dummyPerson, _dummyScheduleRange)))
					.Return(true);
			}
			using (mocks.Playback())
			{
				Assert.IsFalse(target.Remove(dummyPerson));
				Assert.IsTrue(target.Remove(new KeyValuePair<IPerson, IScheduleRange>(dummyPerson, _dummyScheduleRange)));
			}
		}
		
		#endregion
	}
}
