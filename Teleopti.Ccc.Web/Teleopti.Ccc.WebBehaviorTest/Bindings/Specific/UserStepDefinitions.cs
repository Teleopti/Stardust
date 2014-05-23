using System;
using System.Drawing;
using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"I am a user with everyone access")]
		public void GivenIAmAUserWithEveryoneAccess()
		{
			DataMaker.Data().Apply(new AdministratorRoleWithEveryoneData());
		}

		[Given(@"I am an agent")]
		public void GivenIAmAnAgent()
		{
			DataMaker.Data().Apply(new Agent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod());
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent that has a dayoff today according to my contract")]
		public void GivenIAmAnAgentThatHasAContractDayOffToday()
		{
			DataMaker.Data().Apply(new Agent());
			DataMaker.Data().Apply(new SchedulePeriod());
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
			DataMaker.Data().Apply(contractSchedule);
			DataMaker.Data().Apply(new PersonPeriod { ContractSchedule = contractSchedule.ContractSchedule });
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in no team with access to my team")]
		public void GivenIAmAnAgentInNoTeamWithAccessToMyTeam()
		{
			DataMaker.Data().Apply(new Agent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I have analytics data for today")]
		public void GivenIHaveAnalyticsDataForToday()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			var dataSource = new ExistingDatasources(timeZones);
			DataMaker.Analytics().Setup(new EternityAndNotDefinedDate());
			DataMaker.Analytics().Setup(timeZones);
			DataMaker.Analytics().Setup(dates);
			DataMaker.Analytics().Setup(intervals);
			DataMaker.Analytics().Setup(dataSource);
			DataMaker.Analytics().Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));
		}

		[Given(@"I have analytics data for the current week")]
		public void GivenIHaveAnalyticsDataForTheCurrentWeek()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new CurrentWeekDates();
			var intervals = new QuarterOfAnHourInterval();
			var dataSource = new ExistingDatasources(timeZones);
			DataMaker.Analytics().Setup(new EternityAndNotDefinedDate());
			DataMaker.Analytics().Setup(timeZones);
			DataMaker.Analytics().Setup(dates);
			DataMaker.Analytics().Setup(intervals);
			DataMaker.Analytics().Setup(dataSource);
			DataMaker.Analytics().Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));
		}

		[Given(@"I am user with partial access to reports")]
		public void GivenIAmUserWithPartialAccessToReports()
		{
			DataMaker.Data().Apply(new UserWithoutResReportScheduledAndActualAgentsAccess());
		}
		
		[Given(@"I am an agent in a team with access to the whole site")]
		public void GivenIAmAnAgentInATeamWithAccessToTheWholeSite()
		{
			DataMaker.Data().Apply(new AgentWithSiteAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team with access to another site")]
		public void GivenIAmAnAgentInATeamWithAccessToAnotherSite()
		{
			DataMaker.Data().Apply(new AgentWithAnotherSiteAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team")]
		[Given(@"I am an agent in my own team")]
		[Given(@"I am an agent in a team with access to my team")]
		public void GivenIAmAnAgentInATeam()
		{
			DataMaker.Data().Apply(new Agent());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team without access to shift trade requests")]
		public void GivenIAmAnAgentInATeamWithoutAccessToShiftTradeRequests()
		{
			DataMaker.Data().Apply(new AgentWithoutRequestsAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}


		[Given(@"I am an agent in a team with access only to my own data")]
		public void GivenIAmAnAgentWithoutPermissionToSeeMyColleagueSSchedule()
		{
			DataMaker.Data().Apply(new AgentWithoutTeamDataAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am a student agent")]
		public void GivenIAmAStudentAgent()
		{
			DataMaker.Data().Apply(new StudentAgent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod());
		}

		[Given(@"I am an agent without access to student availability")]
		public void GivenIAmAnAgentWithoutAccessToStudentAvailability()
		{
			DataMaker.Data().Apply(new AgentWithoutStudentAvailabilityAccess());
		}

		[Given(@"I am an agent without access to preferences")]
		public void GivenIAmAnAgentWithoutAccessToPreferences()
		{
			DataMaker.Data().Apply(new AgentWithoutPreferencesAccess());
		}

		[Given(@"I am an agent without access to extended preferences")]
		public void GivenIAmAnAgentWithoutAccessToExtendedPreferences()
		{
			DataMaker.Data().Apply(new AgentWithoutExtendedPreferencesAccess());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod());
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I am an agent without access to any requests")]
		public void GivenIAmAnAgentWithoutAccessToAnyRequests()
		{
			DataMaker.Data().Apply(new AgentWithoutRequestsAccess());
		}

		[Given(@"I am an agent with no access to team schedule")]
		public void GivenIAmAnAgentWithNoAccessToTeamSchedule()
		{
			DataMaker.Data().Apply(new AgentWithoutTeamScheduleAccess());
		}

		[Given(@"I have several virtual schedule periods")]
		public void GivenIHaveSeveralVirtualSchedulePeriods()
		{
			DataMaker.Data().Apply(new SchedulePeriod(2));
		}

		[Given(@"I do not have a virtual schedule period")]
		public void GivenIDoNotHaveAVirtualSchedulePeriod()
		{
			DataMaker.Data().Apply(new DoNotHaveVirtualSchedulePeriods());
		}

		[Given(@"I do not have a schedule period")]
		public void GivenIDoNotHaveASchedulePeriod()
		{
			DataMaker.Data().Apply(new DoNotHaveSchedulePeriods());
		}

		[Given(@"I do not have a person period")]
		public void GivenIDoNotHaveAPersonPeriod()
		{
			DataMaker.Data().Apply(new DoNotHavePersonPeriods());
		}

		[Given(@"I have (existing|a) student availability")]
		public void GivenIHaveExistingStudentAvailability(string aOrExisting)
		{
			DataMaker.Data().Apply(new StudentAvailability());
		}

		[Given(@"I have (existing|a) shift category preference")]
		public void GivenIHaveExistingShiftCategoryPreference(string aOrExisting)
		{
			DataMaker.Data().Apply(new ShiftCategoryPreference());
		}

		[Given(@"I have (existing|a) day off preference")]
		public void GivenIHaveExistingDayOffPreference(string aOrExisting)
		{
			DataMaker.Data().Apply(new DayOffPreference());
		}

		[Given(@"I have (existing|a) absence preference")]
		public void GivenIHaveExistingAbsencePreference(string aOrExisting)
		{
			DataMaker.Data().Apply(new AbsencePreference());
		}

		[Given(@"I have a preference with end time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithEndTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreferenceToday(endTimeLimitation));
		}

		[Given(@"I have a preference with start time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithStartTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreferenceToday(startTimeLimitation));
		}

		[Given(@"I have a preference with work time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithWorkTimeLimitationBetweenAnd(int shortest, int longest)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(shortest, 0, 0), new TimeSpan(longest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreferenceToday(workTimeLimitation));
		}


		[Given(@"I have preference for the first category today")]
		public void GivenIHavePreferenceForTheFirstCategoryToday()
		{
			var firstCat = DataMaker.Data().UserData<FirstShiftCategory>();
			var firstCategory = firstCat.ShiftCategory;

			DataMaker.Data().Apply(new ShiftCategoryPreferenceToday {ShiftCategory = firstCategory});
		}

		[Given(@"I have a preference with lunch length limitation of 1 hour today")]
		public void GivenIHaveAPreferenceWithLunchLengthLimitationOf1HourToday()
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(1, 0, 0), new TimeSpan(1, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreferenceToday(workTimeLimitation));
		}

		[Given(@"I have a preference with lunch end time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithLunchEndTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreferenceToday(endTimeLimitation));
		}

		[Given(@"I have a preference with lunch start time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithLunchStartTimeLimitationBetweenAnd(int earliest, int latest)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreferenceToday(startTimeLimitation));
		}

		[Given(@"I have a availabilty with earliest start time at (.*)")]
		public void GivenIHaveAAvailabiltyWithEarliestStartTimeAt(int earliestStart)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliestStart, 0, 0), null);
			DataMaker.Data().Apply(new ExistingAvailability(startTimeLimitation));
		}

		[Given(@"I have a availabilty with latest end time at (.*)")]
		public void GivenIHaveAAvailabiltyWithLatestEndTimeAt(int latestEnd)
		{
			var endTimeLimitation = new EndTimeLimitation(null, new TimeSpan(latestEnd, 0, 0));
			DataMaker.Data().Apply(new ExistingAvailability(endTimeLimitation));
		}

		[Given(@"I have a availabilty with work time between (.*) and (.*) hours")]
		public void GivenIHaveAAvailabiltyWithWorkTimeBetween5And7Hours(int shortest, int longest)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(shortest, 0, 0), new TimeSpan(longest, 0, 0));
			DataMaker.Data().Apply(new ExistingAvailability(workTimeLimitation));
		}

		[Given(@"I have a conflicting preference and availability today")]
		public void GivenIHaveAConflictingPreferenceAndAvailabilityToday()
		{
			var startTimeAvailability = new StartTimeLimitation(new TimeSpan(10, 0, 0), null);
			var startTimePreference = new StartTimeLimitation(null, new TimeSpan(9, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreferenceToday(startTimePreference));
			DataMaker.Data().Apply(new ExistingAvailability(startTimeAvailability));
		}


		[Given(@"My schedule is published")]
		public void GivenMyScheduleIsPublished()
		{
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

		[Given(@"I have a workflow control set")]
		public void GivenIHaveAWorkflowControlSet()
		{
			DataMaker.Data().Apply(new ExistingWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open availability periods")]
		public void GivenIHaveAWorkflowControlSetWithOpenAvailabilityPeriods()
		{
			DataMaker.Data().Apply(new StudentAvailabilityOpenWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed student availability periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedStudentAvailabilityPeriods()
		{
			DataMaker.Data().Apply(new StudentAvailabilityClosedWorkflowControlSet());
		}

		[Given(@"I do not have a workflow control set")]
		public void GivenIDoNotHaveAWorkflowControlSet()
		{
			DataMaker.Data().Apply(new NoWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with student availability periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithStudentAvailabilityPeriodsOpenNextMonth()
		{
			DataMaker.Data().Apply(new StudentAvailabilityOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open standard preference period")]
		public void GivenIHaveAWorkflowControlSetWithOpenStandardPreferencePeriod()
		{
			DataMaker.Data().Apply(new PreferenceOpenWorkflowControlSet());
		}

		[Given(@"I have an open workflow control set with an allowed standard preference")]
		public void GivenIHaveAnOpenWorkflowControlSetWithAnAllowedStandardPreference()
		{
			DataMaker.Data().Apply(new PreferenceOpenWithAllowedPreferencesWorkflowControlSet());
		}

		[Given(@"I have existing standard preference")]
		public void GivenIHaveExistingStandardPreference()
		{
			DataMaker.Data().Apply(new StandardPreference());
		}

		[Given(@"I have 2 existing standard preference")]
		public void GivenIHave2ExistingStandardPreference()
		{
			DataMaker.Data().Apply(new StandardPreference());
			DataMaker.Data().Apply(new AnotherStandardPreference());
		}

		[Given(@"I have a workflow control set with preference periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithPreferencePeriodsOpenNextMonth()
		{
			DataMaker.Data().Apply(new PreferenceOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed preference periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedPreferencePeriods()
		{
			DataMaker.Data().Apply(new PreferenceClosedWorkflowControlSet());
		}

		[Given(@"I have a shift today")]
		public void GivenIHaveAShiftToday()
		{
			DataMaker.Data().Apply(new ShiftToday());
		}


		[Given(@"I have a dayoff today")]
		public void GivenIHaveADayoffToday()
		{
			DataMaker.Data().Apply(new DayOffToday());
		}

		[Given(@"I have a full-day absence today")]
		public void GivenIHaveAFull_DayAbsenceToday()
		{
			DataMaker.Data().Apply(new AbsenceToday());
		}

		[Given(@"I have a full-day contract time absence today")]
		public void GivenIHaveAFull_DayContractTimeAbsenceToday()
		{
			DataMaker.Data().Apply(new AbsenceInContractTimeToday());
		}

		[Given(@"I have a full-day absence today with")]
		public void GivenIHaveAFull_DayAbsenceTodayWith(Table table)
		{
			var absence = table.CreateInstance<AbsenceToday>();
			DataMaker.Data().Apply(absence);
		}

		[Given(@"I have a shift from (.*) to (.*)")]
		public void GivenIHaveAShiftFrom756To1700(string from, string to)
		{
			DataMaker.Data().Apply(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to)));
		}

		[Given(@"I have an activity from (.*) to (.*)")]
		public void GivenIHaveAnActivityFromTo(string from, string to)
		{
			DataMaker.Data().Apply(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to), false));
		}

		[Given(@"I am swedish")]
		public void GivenIAmSwedish()
		{
			DataMaker.Data().Apply(new SwedishCulture());
		}

		[Given(@"I am american")]
		public void GivenIAmUS()
		{
			DataMaker.Data().Apply(new USCulture());
		}

        [Given(@"I am german")]
        public void GivenIAmGerman()
        {
            DataMaker.Data().Apply(new GermanCulture());
        }

		[Given(@"I am englishspeaking swede")]
		public void GivenIAmEnglishSpeakingSwede()
		{
			DataMaker.Data().Apply(new SwedeSpeakingEnglishCulture());
		}

		[Given(@"'?(I)'? am located in [hH]awaii")]
		[Given(@"'?(.*)'? is located in [hH]awaii")]
		public void GivenIAmLocatedInHawaii(string userName)
		{
			DataMaker.Person(userName).Apply(new HawaiiTimeZone());
		}

		[Given(@"'?(I)'? am located in [sS]tockholm")]
		[Given(@"'?(.*)'? is located in [sS]tockholm")]
		public void GivenIAmLocatedInStockholm(string userName)
		{
			DataMaker.Person(userName).Apply(new StockholmTimeZone());
		}

		[Given(@"(.*) am located in '(.*)'")]
		[Given(@"(.*) is located in '(.*)'")]
		public void GivenIsLocatedIn(string name,string location)
		{
			DataMaker.Person(name).Apply(new UserTimeZoneFor(location));
		}


		[Given(@"I have an existing text request")]
		public void GivenIHaveAnExistingTextRequest()
		{
			DataMaker.Data().Apply(new ExistingTextRequest());
		}

		[Given(@"I have a pending text request")]
		public void GivenIHaveAPendingTextRequest()
		{
			DataMaker.Data().Apply(new ExistingPendingTextRequest());
		}

		[Given(@"I have an approved text request")]
		public void GivenIHaveAnApprovedTextRequest()
		{
			DataMaker.Data().Apply(new ExistingApprovedTextRequest());
		}

		[Given(@"I have a denied text request")]
		public void GivenIHaveAnDeniedTextRequest()
		{
			DataMaker.Data().Apply(new ExistingDeniedTextRequest());
		}
		[Given(@"I have more than one page of requests")]
		public void GivenIHaveMoreThanOnePageOfRequests()
		{
			DataMaker.Data().Apply(new MoreThanOnePageOfRequests());
		}

		[Given(@"I have 2 existing request changed on different times")]
		public void GivenIHave2ExistingRequestChangedOnDifferentTimes()
		{
			DataMaker.Data().ApplyLater(new TwoExistingTextRequestChangedOnDifferentTimes());
		}

		[Given(@"I have an existing absence request")]
		public void GivenIHaveAnExistingAbsenceRequest()
		{
			DataMaker.Data().Apply(new ExistingAbsenceRequest());
		}

		[Given(@"I have received a shift trade request")]
		[Given(@"I have created a shift trade request")]
		public void GivenIHaveCreatedAShiftTradeRequest(Table table)
		{
			var existingShiftTrade = table.CreateInstance<ExistingShiftTradeRequest>();
			DataMaker.Data().ApplyLater(existingShiftTrade);
		}

		[Given(@"the site has another team")]
		public void GivenTheSiteHasAnotherTeam()
		{
			DataMaker.Data().Apply(new AnotherTeam());
		}

		[Given(@"the other site has 2 teams")]
		public void GivenTheOtherSiteHas2Teams()
		{
			if (!DataMaker.Data().HasSetup<AnotherSitesTeam>())
				DataMaker.Data().Apply(new AnotherSitesTeam());
			DataMaker.Data().Apply(new AnotherSitesSecondTeam());
		}
		
		[Given(@"I belong to another site's team tomorrow")]
		public void GivenIBelongToAnotherSiteSTeamTomorrow()
		{
			var team = new AnotherSitesTeam();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam, DateOnlyForBehaviorTests.TestToday.AddDays(1)));
		}

		[Given(@"I have a shift bag with start times (.*) to (.*) and end times (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithStartTimesToAndEndTimesTo(int earliestStart, int latestStart, int earliestEnd, int latestEnd)
		{
			DataMaker.Data().Apply(new RuleSetBag(earliestStart, latestStart, earliestEnd, latestEnd));
		}

		[Given(@"I have a shift bag with two categories with shift from (.*) to (.*) and from (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithTwoCategoriesWithShiftFromToAndFromTo(int start1, int end1, int start2, int end2)
		{
			var category1 = new FirstShiftCategory();
			var category2 = new SecondShiftCategory();
			DataMaker.Data().Apply(category1);
			DataMaker.Data().Apply(category2);
			DataMaker.Data().Apply(new RuleSetBagWithTwoCategories(category1, start1, end1, category2, start2, end2));
		}

		[Given(@"I have a shift bag with two categories with shift start from (.*) to (.*) and from (.*) to (.*) and end from (.*) to (.*) and from (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithTwoCategoriesWithShiftStartFromToAndFromToAndEndFromToAndFromTo(int earliestStart1, int latestStart1, int earliestStart2, int latestStart2, int earliestEnd1, int latestEnd1, int earliestEnd2, int latestEnd2)
		{
			var category1 = new FirstShiftCategory();
			var category2 = new SecondShiftCategory();
			DataMaker.Data().Apply(category1);
			DataMaker.Data().Apply(category2);
			DataMaker.Data().Apply(new RuleSetBagWithTwoCategories(category1, earliestStart1, latestStart1, earliestEnd1, latestEnd1, category2, earliestStart2, latestStart2, earliestEnd2, latestEnd2));
		}


		[Given(@"I have a shift bag")]
		public void GivenIHaveAShiftBag()
		{
			DataMaker.Data().Apply(new RuleSetBag(8, 10, 16, 18));
		}

		[Given(@"I have a shift bag with one shift (.*) to (.*) and lunch (.*) to (.*) and one shift (.*) to (.*) and lunch (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithOneShiftToAndLunchToAndOneShiftToAndLunchTo(int start1, int end1, int lunchStart1, int lunchEnd1, int start2, int end2, int lunchStart2, int lunchEnd2)
		{
			DataMaker.Data().Apply(new RuleSetBagWithTwoShiftsAndLunch(start1, end1, lunchStart1, lunchEnd1, start2, end2, lunchStart2, lunchEnd2));
		}

		[Given(@"I am an agent in a team that leaves tomorrow")]
		public void GivenIAmAnAgentThatLeavesTomorrow()
		{
			DataMaker.Data().Apply(new AgentThatLeavesTomorrow());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new ScheduleIsPublished());
		}

	}
}


