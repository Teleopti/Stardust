using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class ShiftTradeMaxSeatValidationTests
	{
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;

		private IActivity _requiresSeatActivity;
		private IActivity _doesNotRequireSeatActivity;
		private ShiftTradeTestHelper _shiftTradeTestHelper;
		private FakeScheduleProjectionReadOnlyActivityProvider _scheduleProjectionReadOnlyActivityProvider;
		private ICurrentScenario _currentScenario;

		protected virtual bool UseReadModel()
		{
			return false;
		}

		[SetUp]
		public void Setup()
		{
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personRepository = new FakePersonRepositoryLegacy2();
			_scheduleStorage = new FakeScheduleStorage();

			_requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			_requiresSeatActivity.RequiresSeat = true;

			_doesNotRequireSeatActivity = ActivityFactory.CreateActivity("Shift_DoesNotRequireSeat");
			_doesNotRequireSeatActivity.RequiresSeat = false;

			_scheduleProjectionReadOnlyActivityProvider = new FakeScheduleProjectionReadOnlyActivityProvider();

			_currentScenario = new FakeCurrentScenario();
			_shiftTradeTestHelper = new ShiftTradeTestHelper(_schedulingResultStateHolder, _scheduleStorage, _personRepository, new FakeBusinessRuleProvider(), _currentScenario, _scheduleProjectionReadOnlyActivityProvider);

			_shiftTradeTestHelper.UseMaxSeatReadModelValidator();
		}

		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceeded()
		{
			var personRequest = doBasicMaxSeatsValidationTest();

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldNotDenyWhenSystemConfigurationExcludesMaxSeatValidation()
		{
			_shiftTradeTestHelper.OverrideShiftTradeGlobalSettings(new ShiftTradeSettings()
			{
				MaxSeatsValidationEnabled = false,
				MaxSeatsValidationSegmentLength = 15
			});

			var personRequest = doBasicMaxSeatsValidationTest();

			Assert.IsFalse(personRequest.IsDenied);
		}

		[Test]
		public void ShouldValidateMaxSeatsAccordingToInterval()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12).AddMinutes(5));
			var afternoonShiftTimePeriodSite1 = new DateTimePeriod(scheduleDate.AddHours(13), scheduleDate.AddHours(17));

			// 12:15-17:00 - Should be accepted in 15 minute interval (default  - so will have 12:00->12:14:59 12:15->12:29:59),
			// not in 30 minute interval (12:00 -> 12:29:59, 12:30 -> ...)
			var afternoonShiftTimePeriodSite2 = new DateTimePeriod(scheduleDate.AddHours(12).AddMinutes(15), scheduleDate.AddHours(17));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriodSite1, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, afternoonShiftTimePeriodSite2, _requiresSeatActivity);

			var personRequest15MinuteInterval = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			_shiftTradeTestHelper.OverrideShiftTradeGlobalSettings(new ShiftTradeSettings()
			{
				MaxSeatsValidationEnabled = true,
				MaxSeatsValidationSegmentLength = 30
			});

			var personRequest30MinuteInterval = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest15MinuteInterval.IsApproved);
			Assert.IsTrue(personRequest30MinuteInterval.IsDenied);
		}

		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceededFromOvernightShift()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var dayBeforeScheduleDate = scheduleDate.AddDays(-1);

			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(2), scheduleDate.AddHours(9));
			var afternoonShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(13), scheduleDate.AddHours(17));
			var overnightShiftTimePeriod = new DateTimePeriod(dayBeforeScheduleDate.AddHours(23), dayBeforeScheduleDate.AddHours(26).AddMinutes(1));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], overnightShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, _requiresSeatActivity);

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceededWhenAgentsAreInDifferentTimezones()
		{
			var chinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
			var newZealandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time"); //26/7

			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var earlyShift = new DateTimePeriod(scheduleDate.AddHours(15), scheduleDate.AddHours(17));// 25/7 9-11 CST   26/7 1 - 3 am NZST
			var lateShift = new DateTimePeriod(scheduleDate.AddHours(17), scheduleDate.AddHours(19));

			var personTo = createPersonWithSiteMaxSeats(1);
			personTo.PermissionInformation.SetDefaultTimeZone(newZealandTimeZone);

			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], earlyShift, _requiresSeatActivity);
			addPersonAssignment(personTo, lateShift, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			personFrom.PermissionInformation.SetDefaultTimeZone(chinaTimeZone);

			addPersonAssignment(personFrom, earlyShift, _requiresSeatActivity);

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldIgnoreRequestThatWillBeTradedAndApproveAsMaxSeatsIsOk()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			// Site One, three people - all three with seats in the morning.  Max seats will be 3.  So traded shift should be able
			// to be traded as one of the existing shifts will be traded away
			var personTo = createPersonWithSiteMaxSeats(3);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly)),
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(peopleInSiteOne[2], morningShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(personTo, morningShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, _requiresSeatActivity);

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], peopleInSiteOne[2], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldAcceptWhenAnAssignmentDoesNotRequireSeats()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(2);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly)),
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, _doesNotRequireSeatActivity);
			addPersonAssignment(peopleInSiteOne[2], morningShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(personTo, morningShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, _requiresSeatActivity);

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], peopleInSiteOne[2], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldAcceptWhenActivitiesInsidePeriodDoNotRequireSeats()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			var startTime = morningShiftTimePeriod.StartDateTime;

			addPersonAssignmentWithMultipleActivities(peopleInSiteOne[1], morningShiftTimePeriod, new List<ActivityAndDateTime>()
			{
				createActivity (true, startTime, startTime.AddHours (1)),
				createActivity (false, startTime.AddHours (1), startTime.AddHours (2)),
				createActivity (true, startTime.AddHours (2), startTime.AddHours (3)),
				createActivity (false, startTime.AddHours (3), startTime.AddHours (4)),
				createActivity (true, startTime.AddHours (4), startTime.AddHours (5))
			});

			addPersonAssignment(personTo, morningShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignmentWithMultipleActivities(personFrom, morningShiftTimePeriod, new List<ActivityAndDateTime>()
			{
				createActivity (false, startTime, startTime.AddHours (1)),
				createActivity (true, startTime.AddHours (1), startTime.AddHours (2)),
				createActivity (false, startTime.AddHours (2), startTime.AddHours (3)),
				createActivity (true, startTime.AddHours (3), startTime.AddHours (4)),
				createActivity (false, startTime.AddHours (4), startTime.AddHours (5))
			});

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldDenyWhenOneActivityInsidePeriodOverlapsViolatingMaxSeats()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			var startTime = morningShiftTimePeriod.StartDateTime;

			addPersonAssignmentWithMultipleActivities(peopleInSiteOne[1], morningShiftTimePeriod, new List<ActivityAndDateTime>()
			{
				createActivity (false,startTime.AddHours(3), startTime.AddHours (4)),
				createActivity (true,startTime.AddHours(4), startTime.AddHours (5))
			});

			addPersonAssignment(personTo, morningShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignmentWithMultipleActivities(personFrom, morningShiftTimePeriod, new List<ActivityAndDateTime>()
			{
				//this activity should conflict with the activity above that begins at 4
				createActivity (true,startTime.AddHours(3), startTime.AddHours (4).AddMinutes (1)),
				createActivity (false,startTime.AddHours(4).AddMinutes (1), startTime.AddHours (5))
			});

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		private IPersonRequest doBasicMaxSeatsValidationTest()
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));
			var afternoonShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(13), scheduleDate.AddHours(17));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				_shiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, _requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriod, _requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, _requiresSeatActivity);

			var personRequest = _shiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);
			return personRequest;
		}

		private void addPersonAssignment(IPerson person, DateTimePeriod dateTimePeriod, IActivity activity = null)
		{
			var dateOnly = new DateOnly(dateTimePeriod.StartDateTime);

			var personAssignment = _shiftTradeTestHelper.AddPersonAssignment(person, dateTimePeriod, activity);
			_scheduleProjectionReadOnlyActivityProvider.AddSiteActivity(new SiteActivity
			{
				PersonId = person.Id.GetValueOrDefault(),
				ActivityId = personAssignment.ShiftLayers.First().Id.GetValueOrDefault(),
				SiteId = person.MyTeam(dateOnly).Site.Id.Value,
				StartDateTime = dateTimePeriod.StartDateTime,
				EndDateTime = dateTimePeriod.EndDateTime,
				RequiresSeat = activity.RequiresSeat
			});
		}

		private void addPersonAssignmentWithMultipleActivities(IPerson person, DateTimePeriod dateTimePeriod, IList<ActivityAndDateTime> activityDefinitions)
		{
			var dateOnly = new DateOnly(dateTimePeriod.StartDateTime);
			var scenario = _currentScenario.Current();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, dateOnly).WithId();
			foreach (var activityDefinition in activityDefinitions)
			{
				personAssignment.AddActivity(activityDefinition.Activity, activityDefinition.Period);
				_scheduleProjectionReadOnlyActivityProvider.AddSiteActivity(new SiteActivity
				{
					PersonId = person.Id.GetValueOrDefault(),
					ActivityId = activityDefinition.Activity.Id.Value,
					SiteId = person.MyTeam(dateOnly).Site.Id.Value,
					StartDateTime = activityDefinition.Period.StartDateTime,
					EndDateTime = activityDefinition.Period.EndDateTime,
					RequiresSeat = activityDefinition.Activity.RequiresSeat
				});
			}

			_scheduleStorage.Add(personAssignment);
		}

		private IPerson createPersonWithSiteMaxSeats(int maxSeats)
		{
			var workControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			var startDate = new DateOnly(2016, 1, 1);

			var team = TeamFactory.CreateTeam("team", "site");
			team.Site.SetId(Guid.NewGuid());
			team.Site.MaxSeats = maxSeats;

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			((Person)person).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			person.WorkflowControlSet = workControlSet;
			_personRepository.Add(person);

			return person;
		}

		private ActivityAndDateTime createActivity(bool requiresSeat, DateTime startDateTime, DateTime endDateTime)
		{
			return new ActivityAndDateTime
			{
				Activity = requiresSeat ? _requiresSeatActivity : _doesNotRequireSeatActivity,
				Period = new DateTimePeriod(startDateTime, endDateTime)
			};
		}
	}

	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	public class ShiftTradeMaxSeatReadModelValidationTests : ShiftTradeMaxSeatValidationTests
	{
		protected override bool UseReadModel()
		{
			return true;
		}
	}
}