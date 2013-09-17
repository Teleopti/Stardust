using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"I am a user with everyone access")]
		public void GivenIAmAUserWithEveryoneAccess()
		{
			DataMaker.Data().Setup(new AdministratorRoleWithEveryoneData());
		}

		[Given(@"I am an agent")]
		public void GivenIAmAnAgent()
		{
			DataMaker.Data().Setup(new Agent());
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod());
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent that has a dayoff today according to my contract")]
		public void GivenIAmAnAgentThatHasAContractDayOffToday()
		{
			DataMaker.Data().Setup(new Agent());
			DataMaker.Data().Setup(new SchedulePeriod());
			var contractSchedule = new ContractScheduleFromTable
			                       	{
			                       		MondayWorkDay = false,
			                       		TuesdayWorkDay = false,
			                       		WednesdayWorkDay = false,
			                       		ThursdayWorkDay = false,
			                       		FridayWorkDay = false,
			                       		SaturdayWorkDay = false,
			                       		SundayWorkDay = false
			                       	};
			DataMaker.Data().Setup(contractSchedule);
			DataMaker.Data().Setup(new PersonPeriod { ContractSchedule = contractSchedule });
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in no team with access to my team")]
		public void GivenIAmAnAgentInNoTeamWithAccessToMyTeam()
		{
			DataMaker.Data().Setup(new Agent());
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I have analytics data for today")]
		public void GivenIHaveAnalyticsDataForToday()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			var dataSource = new ExistingDatasources(timeZones);
			DataMaker.Data().Setup(new EternityAndNotDefinedDate());
			DataMaker.Data().Setup(timeZones);
			DataMaker.Data().Setup(dates);
			DataMaker.Data().Setup(intervals);
			DataMaker.Data().Setup(dataSource);
			DataMaker.Data().Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));
		}

		[Given(@"I have analytics data for the current week")]
		public void GivenIHaveAnalyticsDataForTheCurrentWeek()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new CurrentWeekDates();
			var intervals = new QuarterOfAnHourInterval();
			var dataSource = new ExistingDatasources(timeZones);
			DataMaker.Data().Setup(new EternityAndNotDefinedDate());
			DataMaker.Data().Setup(timeZones);
			DataMaker.Data().Setup(dates);
			DataMaker.Data().Setup(intervals);
			DataMaker.Data().Setup(dataSource);
			DataMaker.Data().Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));
		}

		[Given(@"I am user with partial access to reports")]
		public void GivenIAmUserWithPartialAccessToReports()
		{
			DataMaker.Data().Setup(new UserWithoutResReportScheduledAndActualAgentsAccess());
		}
		
		[Given(@"I am an agent in a team with access to the whole site")]
		public void GivenIAmAnAgentInATeamWithAccessToTheWholeSite()
		{
			DataMaker.Data().Setup(new AgentWithSiteAccess());
			var team = new Team();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team with access to another site")]
		public void GivenIAmAnAgentInATeamWithAccessToAnotherSite()
		{
			DataMaker.Data().Setup(new AgentWithAnotherSiteAccess());
			var team = new Team();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team")]
		[Given(@"I am an agent in my own team")]
		[Given(@"I am an agent in a team with access to my team")]
		public void GivenIAmAnAgentInATeam()
		{
			DataMaker.Data().Setup(new Agent());
			var team = new Team();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team with access only to my own data")]
		public void GivenIAmAnAgentWithoutPermissionToSeeMyColleagueSSchedule()
		{
			DataMaker.Data().Setup(new AgentWithoutTeamDataAccess());
			var team = new Team();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am a student agent")]
		public void GivenIAmAStudentAgent()
		{
			DataMaker.Data().Setup(new StudentAgent());
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod());
		}

		[Given(@"I am an agent without access to student availability")]
		public void GivenIAmAnAgentWithoutAccessToStudentAvailability()
		{
			DataMaker.Data().Setup(new AgentWithoutStudentAvailabilityAccess());
		}

		[Given(@"I am an agent without access to preferences")]
		public void GivenIAmAnAgentWithoutAccessToPreferences()
		{
			DataMaker.Data().Setup(new AgentWithoutPreferencesAccess());
		}

		[Given(@"I am an agent without access to extended preferences")]
		public void GivenIAmAnAgentWithoutAccessToExtendedPreferences()
		{
			DataMaker.Data().Setup(new AgentWithoutExtendedPreferencesAccess());
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod());
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent without access to any requests")]
		public void GivenIAmAnAgentWithoutAccessToAnyRequests()
		{
			DataMaker.Data().Setup(new AgentWithoutRequestsAccess());
		}

		[Given(@"I am an agent with no access to team schedule")]
		public void GivenIAmAnAgentWithNoAccessToTeamSchedule()
		{
			DataMaker.Data().Setup(new AgentWithoutTeamScheduleAccess());
		}

		[Given(@"I have several virtual schedule periods")]
		public void GivenIHaveSeveralVirtualSchedulePeriods()
		{
			DataMaker.Data().Setup(new SchedulePeriod(2));
		}

		[Given(@"I do not have a virtual schedule period")]
		public void GivenIDoNotHaveAVirtualSchedulePeriod()
		{
			DataMaker.Data().Setup(new DoNotHaveVirtualSchedulePeriods());
		}

		[Given(@"I do not have a schedule period")]
		public void GivenIDoNotHaveASchedulePeriod()
		{
			DataMaker.Data().Setup(new DoNotHaveSchedulePeriods());
		}

		[Given(@"I do not have a person period")]
		public void GivenIDoNotHaveAPersonPeriod()
		{
			DataMaker.Data().Setup(new DoNotHavePersonPeriods());
		}

		[Given(@"I have (existing|a) student availability")]
		public void GivenIHaveExistingStudentAvailability(string aOrExisting)
		{
			DataMaker.Data().Setup(new StudentAvailability());
		}

		[Given(@"I have (existing|a) shift category preference")]
		public void GivenIHaveExistingShiftCategoryPreference(string aOrExisting)
		{
			DataMaker.Data().Setup(new ShiftCategoryPreference());
		}

		[Given(@"I have (existing|a) day off preference")]
		public void GivenIHaveExistingDayOffPreference(string aOrExisting)
		{
			DataMaker.Data().Setup(new DayOffPreference());
		}

		[Given(@"I have (existing|a) absence preference")]
		public void GivenIHaveExistingAbsencePreference(string aOrExisting)
		{
			DataMaker.Data().Setup(new AbsencePreference());
		}

		[Given(@"I have a preference with end time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithEndTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Setup(new ExistingExtendedPreferenceToday(endTimeLimitation));
		}

		[Given(@"I have a preference with start time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithStartTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Setup(new ExistingExtendedPreferenceToday(startTimeLimitation));
		}

		[Given(@"I have a preference with work time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithWorkTimeLimitationBetweenAnd(int shortest, int longest)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(shortest, 0, 0), new TimeSpan(longest, 0, 0));
			DataMaker.Data().Setup(new ExistingExtendedPreferenceToday(workTimeLimitation));
		}


		[Given(@"I have preference for the first category today")]
		public void GivenIHavePreferenceForTheFirstCategoryToday()
		{
			var firstCat = DataMaker.Data().UserData<FirstShiftCategory>();
			var firstCategory = firstCat.ShiftCategory;

			DataMaker.Data().Setup(new ShiftCategoryPreferenceToday {ShiftCategory = firstCategory});
		}

		[Given(@"I have a preference with lunch length limitation of 1 hour today")]
		public void GivenIHaveAPreferenceWithLunchLengthLimitationOf1HourToday()
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(1, 0, 0), new TimeSpan(1, 0, 0));
			DataMaker.Data().Setup(new ExistingLunchPreferenceToday(workTimeLimitation));
		}

		[Given(@"I have a preference with lunch end time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithLunchEndTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Setup(new ExistingLunchPreferenceToday(endTimeLimitation));
		}

		[Given(@"I have a preference with lunch start time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithLunchStartTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Setup(new ExistingLunchPreferenceToday(startTimeLimitation));
		}

		[Given(@"I have a availabilty with earliest start time at (.*)")]
		public void GivenIHaveAAvailabiltyWithEarliestStartTimeAt(int earliestStart)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliestStart, 0, 0), null);
			DataMaker.Data().Setup(new ExistingAvailability(startTimeLimitation));
		}

		[Given(@"I have a availabilty with latest end time at (.*)")]
		public void GivenIHaveAAvailabiltyWithLatestEndTimeAt(int latestEnd)
		{
			var endTimeLimitation = new EndTimeLimitation(null, new TimeSpan(latestEnd, 0, 0));
			DataMaker.Data().Setup(new ExistingAvailability(endTimeLimitation));
		}

		[Given(@"I have a availabilty with work time between (.*) and (.*) hours")]
		public void GivenIHaveAAvailabiltyWithWorkTimeBetween5And7Hours(int shortest, int longest)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(shortest, 0, 0), new TimeSpan(longest, 0, 0));
			DataMaker.Data().Setup(new ExistingAvailability(workTimeLimitation));
		}

		[Given(@"I have a conflicting preference and availability today")]
		public void GivenIHaveAConflictingPreferenceAndAvailabilityToday()
		{
			var startTimeAvailability = new StartTimeLimitation(new TimeSpan(10, 0, 0), null);
			var startTimePreference = new StartTimeLimitation(null, new TimeSpan(9, 0, 0));
			DataMaker.Data().Setup(new ExistingExtendedPreferenceToday(startTimePreference));
			DataMaker.Data().Setup(new ExistingAvailability(startTimeAvailability));
		}


		[Given(@"My schedule is published")]
		public void GivenMyScheduleIsPublished()
		{
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

		[Given(@"I have a workflow control set")]
		public void GivenIHaveAWorkflowControlSet()
		{
			DataMaker.Data().Setup(new ExistingWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open availability periods")]
		public void GivenIHaveAWorkflowControlSetWithOpenAvailabilityPeriods()
		{
			DataMaker.Data().Setup(new StudentAvailabilityOpenWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed student availability periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedStudentAvailabilityPeriods()
		{
			DataMaker.Data().Setup(new StudentAvailabilityClosedWorkflowControlSet());
		}

		[Given(@"I do not have a workflow control set")]
		public void GivenIDoNotHaveAWorkflowControlSet()
		{
			DataMaker.Data().Setup(new NoWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with student availability periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithStudentAvailabilityPeriodsOpenNextMonth()
		{
			DataMaker.Data().Setup(new StudentAvailabilityOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open standard preference period")]
		public void GivenIHaveAWorkflowControlSetWithOpenStandardPreferencePeriod()
		{
			DataMaker.Data().Setup(new PreferenceOpenWorkflowControlSet());
		}

		[Given(@"I have an open workflow control set with an allowed standard preference")]
		public void GivenIHaveAnOpenWorkflowControlSetWithAnAllowedStandardPreference()
		{
			DataMaker.Data().Setup(new PreferenceOpenWithAllowedPreferencesWorkflowControlSet());
		}

		[Given(@"I have existing standard preference")]
		public void GivenIHaveExistingStandardPreference()
		{
			DataMaker.Data().Setup(new StandardPreference());
		}

		[Given(@"I have 2 existing standard preference")]
		public void GivenIHave2ExistingStandardPreference()
		{
			DataMaker.Data().Setup(new StandardPreference());
			DataMaker.Data().Setup(new AnotherStandardPreference());
		}

		[Given(@"I have a workflow control set with preference periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithPreferencePeriodsOpenNextMonth()
		{
			DataMaker.Data().Setup(new PreferenceOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed preference periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedPreferencePeriods()
		{
			DataMaker.Data().Setup(new PreferenceClosedWorkflowControlSet());
		}

		[Given(@"I have a shift today")]
		public void GivenIHaveAShiftToday()
		{
			DataMaker.Data().Setup(new ShiftToday());
		}


		[Given(@"I have a dayoff today")]
		public void GivenIHaveADayoffToday()
		{
			DataMaker.Data().Setup(new DayOffToday());
		}

		[Given(@"I have a full-day absence today")]
		public void GivenIHaveAFull_DayAbsenceToday()
		{
			DataMaker.Data().Setup(new AbsenceToday());
		}

		[Given(@"I have a full-day absence today with")]
		public void GivenIHaveAFull_DayAbsenceTodayWith(Table table)
		{
			var absence = table.CreateInstance<AbsenceToday>();
			DataMaker.Data().Setup(absence);
		}

		[Given(@"I have a shift from (.*) to (.*)")]
		public void GivenIHaveAShiftFrom756To1700(string from, string to)
		{
			DataMaker.Data().Setup(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to)));
		}

		[Given(@"I have an activity from (.*) to (.*)")]
		public void GivenIHaveAnActivityFromTo(string from, string to)
		{
			DataMaker.Data().Setup(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to), false));
		}

		[Given(@"I am swedish")]
		public void GivenIAmSwedish()
		{
			DataMaker.Data().SetupCulture(new SwedishCulture());
		}

		[Given(@"I am american")]
		public void GivenIAmUS()
		{
			DataMaker.Data().SetupCulture(new USCulture());
		}

		[Given(@"'?(I)'? am located in [hH]awaii")]
		[Given(@"'?(.*)'? is located in [hH]awaii")]
		public void GivenIAmLocatedInHawaii(string userName)
		{
			DataMaker.Person(userName).SetupTimeZone(new HawaiiTimeZone());
		}

		[Given(@"'?(I)'? am located in [sS]tockholm")]
		[Given(@"'?(.*)'? is located in [sS]tockholm")]
		public void GivenIAmLocatedInStockholm(string userName)
		{
			DataMaker.Person(userName).SetupTimeZone(new StockholmTimeZone());
		}


		[Given(@"I have an existing text request")]
		public void GivenIHaveAnExistingTextRequest()
		{
			DataMaker.Data().Setup(new ExistingTextRequest());
		}

		[Given(@"I have no existing requests")]
		public void GivenIHaveNoExistingRequests()
		{

		}

		[Given(@"I have a pending text request")]
		public void GivenIHaveAPendingTextRequest()
		{
			DataMaker.Data().Setup(new ExistingPendingTextRequest());
		}

		[Given(@"I have an approved text request")]
		public void GivenIHaveAnApprovedTextRequest()
		{
			DataMaker.Data().Setup(new ExistingApprovedTextRequest());
		}

		[Given(@"I have a denied text request")]
		public void GivenIHaveAnDeniedTextRequest()
		{
			DataMaker.Data().Setup(new ExistingDeniedTextRequest());
		}
		[Given(@"I have more than one page of requests")]
		public void GivenIHaveMoreThanOnePageOfRequests()
		{
			DataMaker.Data().Setup(new MoreThanOnePageOfRequests());
		}

		[Given(@"I have 2 existing request changed on different times")]
		public void GivenIHave2ExistingRequestChangedOnDifferentTimes()
		{
			DataMaker.Data().Setup(new TwoExistingTextRequestChangedOnDifferentTimes());
		}

		[Given(@"I have an existing absence request")]
		public void GivenIHaveAnExistingAbsenceRequest()
		{
			DataMaker.Data().Setup(new ExistingAbsenceRequest());
		}

		[Given(@"I have received a shift trade request")]
		[Given(@"I have created a shift trade request")]
		public void GivenIHaveCreatedAShiftTradeRequest(Table table)
		{
			var existingShiftTrade = table.CreateInstance<ExistingShiftTradeRequest>();
			DataMaker.Data().Setup(existingShiftTrade);
		}

		[Given(@"the site has another team")]
		public void GivenTheSiteHasAnotherTeam()
		{
			DataMaker.Data().Setup(new AnotherTeam());
		}

		[Given(@"the other site has 2 teams")]
		public void GivenTheOtherSiteHas2Teams()
		{
			if (!DataMaker.Data().HasSetup<AnotherSitesTeam>())
				DataMaker.Data().Setup(new AnotherSitesTeam());
			DataMaker.Data().Setup(new AnotherSitesSecondTeam());
		}
		
		[Given(@"I belong to another site's team tomorrow")]
		public void GivenIBelongToAnotherSiteSTeamTomorrow()
		{
			var team = new AnotherSitesTeam();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam, DateOnlyForBehaviorTests.TestToday.AddDays(1)));
		}

		[Given(@"I have a shift bag with start times (.*) to (.*) and end times (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithStartTimesToAndEndTimesTo(int earliestStart, int latestStart, int earliestEnd, int latestEnd)
		{
			DataMaker.Data().Setup(new RuleSetBag(earliestStart, latestStart, earliestEnd, latestEnd));
		}

		[Given(@"I have a shift bag with two categories with shift from (.*) to (.*) and from (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithTwoCategoriesWithShiftFromToAndFromTo(int start1, int end1, int start2, int end2)
		{
			var category1 = new FirstShiftCategory();
			var category2 = new SecondShiftCategory();
			DataMaker.Data().Setup(category1);
			DataMaker.Data().Setup(category2);
			DataMaker.Data().Setup(new RuleSetBagWithTwoCategories(category1, start1, end1, category2, start2, end2));
		}

		[Given(@"I have a shift bag with two categories with shift start from (.*) to (.*) and from (.*) to (.*) and end from (.*) to (.*) and from (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithTwoCategoriesWithShiftStartFromToAndFromToAndEndFromToAndFromTo(int earliestStart1, int latestStart1, int earliestStart2, int latestStart2, int earliestEnd1, int latestEnd1, int earliestEnd2, int latestEnd2)
		{
			var category1 = new FirstShiftCategory();
			var category2 = new SecondShiftCategory();
			DataMaker.Data().Setup(category1);
			DataMaker.Data().Setup(category2);
			DataMaker.Data().Setup(new RuleSetBagWithTwoCategories(category1, earliestStart1, latestStart1, earliestEnd1, latestEnd1, category2, earliestStart2, latestStart2, earliestEnd2, latestEnd2));
		}


		[Given(@"I have a shift bag")]
		public void GivenIHaveAShiftBag()
		{
			DataMaker.Data().Setup(new RuleSetBag(8, 10, 16, 18));
		}

		[Given(@"I have a shift bag with one shift (.*) to (.*) and lunch (.*) to (.*) and one shift (.*) to (.*) and lunch (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithOneShiftToAndLunchToAndOneShiftToAndLunchTo(int start1, int end1, int lunchStart1, int lunchEnd1, int start2, int end2, int lunchStart2, int lunchEnd2)
		{
			DataMaker.Data().Setup(new RuleSetBagWithTwoShiftsAndLunch(start1, end1, lunchStart1, lunchEnd1, start2, end2, lunchStart2, lunchEnd2));
		}

		[Given(@"I am an agent in a team that leaves tomorrow")]
		public void GivenIAmAnAgentThatLeavesTomorrow()
		{
			DataMaker.Data().Setup(new AgentThatLeavesTomorrow());
			var team = new Team();
			DataMaker.Data().Setup(team);
			DataMaker.Data().Setup(new SchedulePeriod());
			DataMaker.Data().Setup(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Setup(new ScheduleIsPublished());
		}

	}
}


