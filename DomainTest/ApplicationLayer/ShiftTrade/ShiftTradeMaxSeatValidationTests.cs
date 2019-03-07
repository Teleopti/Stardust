using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[DomainTest]
	[NoDefaultData]
	public class ShiftTradeMaxSeatValidationTests : IIsolateSystem
	{
		public IPersonRepository PersonRepository;
		public ShiftTradeTestHelper ShiftTradeTestHelper;
		public FakeScheduleProjectionReadOnlyActivityProvider ScheduleProjectionReadOnlyActivityProvider;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		protected virtual bool UseReadModel()
		{
			return false;
		}
		
		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceeded()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat").WithId();
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			var personRequest = doBasicMaxSeatsValidationTest(requiresSeatActivity );

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldNotDenyWhenSystemConfigurationExcludesMaxSeatValidation()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat").WithId();
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			ShiftTradeTestHelper.OverrideShiftTradeGlobalSettings(new ShiftTradeSettings()
			{
				MaxSeatsValidationEnabled = false,
				MaxSeatsValidationSegmentLength = 15
			});

			var personRequest = doBasicMaxSeatsValidationTest(requiresSeatActivity);

			Assert.IsFalse(personRequest.IsDenied);
		}

		[Test]
		public void ShouldValidateMaxSeatsAccordingToInterval()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
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
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriodSite1, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, afternoonShiftTimePeriodSite2, requiresSeatActivity);

			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(MaximumWorkdayRule).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};

			var personRequest15MinuteInterval = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate, new[] { shiftTradeBusinessRuleConfig });

			ShiftTradeTestHelper.OverrideShiftTradeGlobalSettings(new ShiftTradeSettings
			{
				MaxSeatsValidationEnabled = true,
				MaxSeatsValidationSegmentLength = 30
			});

			var personRequest30MinuteInterval = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest15MinuteInterval.IsApproved);
			Assert.IsTrue(personRequest30MinuteInterval.IsDenied);
		}

		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceededFromOvernightShift()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
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
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], overnightShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, requiresSeatActivity);

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldDenyWhenMaxSeatsWillBeExceededWhenAgentsAreInDifferentTimezones()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
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
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], earlyShift, requiresSeatActivity);
			addPersonAssignment(personTo, lateShift, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			personFrom.PermissionInformation.SetDefaultTimeZone(chinaTimeZone);

			addPersonAssignment(personFrom, earlyShift, requiresSeatActivity);

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		[Test]
		public void ShouldIgnoreRequestThatWillBeTradedAndApproveAsMaxSeatsIsOk()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;
			
			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);
			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			// Site One, three people - all three with seats in the morning.  Max seats will be 3.  So traded shift should be able
			// to be traded as one of the existing shifts will be traded away
			var personTo = createPersonWithSiteMaxSeats(3);
			var peopleInSiteOne = new[]
			{
				personTo,
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly)),
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(peopleInSiteOne[2], morningShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(personTo, morningShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, requiresSeatActivity);

			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(MaximumWorkdayRule).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], peopleInSiteOne[2], personFrom }, scheduleDate,new[] { shiftTradeBusinessRuleConfig });

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldAcceptWhenAnAssignmentDoesNotRequireSeats()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;

			var doesNotRequireSeatActivity = ActivityFactory.CreateActivity("Shift_DoesNotRequireSeat");
			doesNotRequireSeatActivity.RequiresSeat = false;

			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(2);
			var peopleInSiteOne = new[]
			{
				personTo,
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly)),
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, doesNotRequireSeatActivity);
			addPersonAssignment(peopleInSiteOne[2], morningShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(personTo, morningShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, requiresSeatActivity);

			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(MaximumWorkdayRule).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], peopleInSiteOne[2], personFrom }, scheduleDate,new[] { shiftTradeBusinessRuleConfig });

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldAcceptWhenActivitiesInsidePeriodDoNotRequireSeats()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;

			var doesNotRequireSeatActivity = ActivityFactory.CreateActivity("Shift_DoesNotRequireSeat");
			doesNotRequireSeatActivity.RequiresSeat = false;

			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			var startTime = morningShiftTimePeriod.StartDateTime;

			addPersonAssignmentWithMultipleActivities(peopleInSiteOne[1], morningShiftTimePeriod, new List<ActivityAndDateTime>
			{
				createActivity (true, startTime, startTime.AddHours (1), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (false, startTime.AddHours (1), startTime.AddHours (2), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (true, startTime.AddHours (2), startTime.AddHours (3), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (false, startTime.AddHours (3), startTime.AddHours (4), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (true, startTime.AddHours (4), startTime.AddHours (5), requiresSeatActivity, doesNotRequireSeatActivity)
			});

			addPersonAssignment(personTo, morningShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignmentWithMultipleActivities(personFrom, morningShiftTimePeriod, new List<ActivityAndDateTime>
			{
				createActivity (false, startTime, startTime.AddHours (1), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (true, startTime.AddHours (1), startTime.AddHours (2), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (false, startTime.AddHours (2), startTime.AddHours (3), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (true, startTime.AddHours (3), startTime.AddHours (4), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (false, startTime.AddHours (4), startTime.AddHours (5), requiresSeatActivity, doesNotRequireSeatActivity)
			});
			var shiftTradeBusinessRuleConfig = new ShiftTradeBusinessRuleConfig
			{
				BusinessRuleType = typeof(MaximumWorkdayRule).FullName,
				Enabled = false,
				HandleOptionOnFailed = RequestHandleOption.AutoDeny
			};
			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate, new[] { shiftTradeBusinessRuleConfig });

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldDenyWhenOneActivityInsidePeriodOverlapsViolatingMaxSeats()
		{
			var requiresSeatActivity = ActivityFactory.CreateActivity("Shift_RequiresSeat");
			requiresSeatActivity.RequiresSeat = true;

			var doesNotRequireSeatActivity = ActivityFactory.CreateActivity("Shift_DoesNotRequireSeat");
			doesNotRequireSeatActivity.RequiresSeat = false;

			ScenarioRepository.Has("Default");

			ShiftTradeTestHelper.UseMaxSeatReadModelValidator();
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			var startTime = morningShiftTimePeriod.StartDateTime;

			addPersonAssignmentWithMultipleActivities(peopleInSiteOne[1], morningShiftTimePeriod, new List<ActivityAndDateTime>
			{
				createActivity (false,startTime.AddHours(3), startTime.AddHours (4), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (true,startTime.AddHours(4), startTime.AddHours (5), requiresSeatActivity, doesNotRequireSeatActivity)
			});

			addPersonAssignment(personTo, morningShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignmentWithMultipleActivities(personFrom, morningShiftTimePeriod, new List<ActivityAndDateTime>
			{
				//this activity should conflict with the activity above that begins at 4
				createActivity (true,startTime.AddHours(3), startTime.AddHours (4).AddMinutes (1), requiresSeatActivity, doesNotRequireSeatActivity),
				createActivity (false,startTime.AddHours(4).AddMinutes (1), startTime.AddHours (5), requiresSeatActivity, doesNotRequireSeatActivity)
			});

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly, new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);

			Assert.IsTrue(personRequest.IsDenied);
		}

		private IPersonRequest doBasicMaxSeatsValidationTest(IActivity requiresSeatActivity)
		{
			var scheduleDate = new DateTime(2016, 7, 25, 0, 0, 0, DateTimeKind.Utc);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var morningShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(7), scheduleDate.AddHours(12));
			var afternoonShiftTimePeriod = new DateTimePeriod(scheduleDate.AddHours(13), scheduleDate.AddHours(17));

			var personTo = createPersonWithSiteMaxSeats(1);
			var peopleInSiteOne = new[]
			{
				personTo,
				ShiftTradeTestHelper.CreatePersonInTeam (personTo.MyTeam (scheduleDateOnly))
			};

			addPersonAssignment(peopleInSiteOne[1], morningShiftTimePeriod, requiresSeatActivity);
			addPersonAssignment(personTo, afternoonShiftTimePeriod, requiresSeatActivity);

			var personFrom = createPersonWithSiteMaxSeats(1);
			addPersonAssignment(personFrom, morningShiftTimePeriod, requiresSeatActivity);

			var personRequest = ShiftTradeTestHelper.PrepareAndExecuteRequest(personTo, personFrom, scheduleDateOnly,
				new[] { personTo, peopleInSiteOne[1], personFrom }, scheduleDate);
			return personRequest;
		}

		private void addPersonAssignment(IPerson person, DateTimePeriod dateTimePeriod, IActivity activity = null)
		{
			var dateOnly = new DateOnly(dateTimePeriod.StartDateTime);

			var personAssignment = ShiftTradeTestHelper.AddPersonAssignment(person, dateTimePeriod, activity);
			ScheduleProjectionReadOnlyActivityProvider.AddSiteActivity(new SiteActivity
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
			var scenario = ScenarioRepository.LoadDefaultScenario();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignment(person, scenario, dateOnly).WithId();
			foreach (var activityDefinition in activityDefinitions)
			{
				personAssignment.AddActivity(activityDefinition.Activity, activityDefinition.Period);
				ScheduleProjectionReadOnlyActivityProvider.AddSiteActivity(new SiteActivity
				{
					PersonId = person.Id.GetValueOrDefault(),
					ActivityId = activityDefinition.Activity.Id.Value,
					SiteId = person.MyTeam(dateOnly).Site.Id.Value,
					StartDateTime = activityDefinition.Period.StartDateTime,
					EndDateTime = activityDefinition.Period.EndDateTime,
					RequiresSeat = activityDefinition.Activity.RequiresSeat
				});
			}

			PersonAssignmentRepository.Add(personAssignment);
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
			PersonRepository.Add(person);

			return person;
		}

		private ActivityAndDateTime createActivity(bool requiresSeat, DateTime startDateTime, DateTime endDateTime, IActivity requiresSeatActivity, IActivity doesNotRequireSeatActivity)
		{
			return new ActivityAndDateTime
			{
				Activity = requiresSeat ? requiresSeatActivity : doesNotRequireSeatActivity,
				Period = new DateTimePeriod(startDateTime, endDateTime)
			};
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ShiftTradeTestHelper>().For<ShiftTradeTestHelper>();
			isolate.UseTestDouble<FakeBusinessRuleProvider>().For<IBusinessRuleProvider>();

			var activityProvider = new FakeScheduleProjectionReadOnlyActivityProvider();
			isolate.UseTestDouble(activityProvider).For<IScheduleProjectionReadOnlyActivityProvider>();
		}
	}

	public class ShiftTradeMaxSeatReadModelValidationTests : ShiftTradeMaxSeatValidationTests
	{
		protected override bool UseReadModel()
		{
			return true;
		}
	}
}