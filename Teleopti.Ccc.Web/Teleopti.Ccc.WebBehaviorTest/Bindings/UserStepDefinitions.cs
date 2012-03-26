using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"I am a user with access to all areas")]
		public void GivenIAmAUserWithAccessToAllAreas()
		{
			UserFactory.User().Setup(new Administrator());
		}

		[Given(@"I am a user with everyone access")]
		public void GivenIAmAUserWithEveryoneAccess()
		{
			UserFactory.User().Setup(new AdministratorRoleWithEveryoneData());
		}

		[Given(@"I am a user with access only to MyTime")]
		public void GivenIAmAUserWithAccessOnlyToMyTime()
		{
			UserFactory.User().Setup(new Agent());
		}

		[Given(@"I am a user with access only to Mobile Reports")]
		public void GivenIAmAUserWithAccessOnlyToMobileReports()
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I am an agent")]
		public void GivenIAmAnAgent()
		{
			UserFactory.User().Setup(new Agent());
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod());
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent that has a dayoff today according to my contract")]
		public void GivenIAmAnAgentThatHasAContractDayOffToday()
		{
			UserFactory.User().Setup(new Agent());
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod {ContractSchedule = TestData.DayOffTodayContractSchedule});
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in no team with access to my team")]
		public void GivenIAmAnAgentInNoTeamWithAccessToMyTeam()
		{
			UserFactory.User().Setup(new Agent());
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am a supervisor")]
		public void GivenIAmASupervisorWithMobile()
		{
			UserFactory.User().Setup(new Supervisor());
		}

		[Given(@"I am user without permission to MobileReports")]
		public void GivenIAmUserWithoutPermissionToMobileReports()
		{
			UserFactory.User().Setup(new UserWithoutMobileReportsAccess());
		}


		[Given(@"I am user with partial access to reports")]
		public void GivenIAmUserWithPartialAccessToReports()
		{
			UserFactory.User().Setup(new UserWithoutResReportServiceLevelAndAgentsReadyAccess());
		}
		
		[Given(@"I am an agent in a team with access to the whole site")]
		public void GivenIAmAnAgentInATeamWithAccessToTheWholeSite()
		{
			UserFactory.User().Setup(new AgentWithSiteAccess());
			var team = new Team();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam));
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team with access to another site")]
		public void GivenIAmAnAgentInATeamWithAccessToAnotherSite()
		{
			UserFactory.User().Setup(new AgentWithAnotherSiteAccess());
			var team = new Team();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam));
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team")]
		[Given(@"I am an agent in my own team")]
		[Given(@"I am an agent in a team with access to my team")]
		public void GivenIAmAnAgentInATeam()
		{
			UserFactory.User().Setup(new Agent());
			var team = new Team();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam));
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am an agent in a team with access only to my own data")]
		public void GivenIAmAnAgentWithoutPermissionToSeeMyColleagueSSchedule()
		{
			UserFactory.User().Setup(new AgentWithoutTeamDataAccess());
			var team = new Team();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam));
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I am a student agent")]
		public void GivenIAmAStudentAgent()
		{
			UserFactory.User().Setup(new StudentAgent());
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod());
		}

		[Given(@"I am an agent without access to student availability")]
		public void GivenIAmAnAgentWithoutAccessToStudentAvailability()
		{
			UserFactory.User().Setup(new AgentWithoutStudentAvailabilityAccess());
		}

		[Given(@"I am an agent without access to preferences")]
		public void GivenIAmAnAgentWithoutAccessToPreferences()
		{
			UserFactory.User().Setup(new AgentWithoutPreferencesAccess());
		}

		[Given(@"I am an agent without access to any requests")]
		public void GivenIAmAnAgentWithoutAccessToAnyRequests()
		{
			UserFactory.User().Setup(new AgentWithoutRequestsAccess());
		}

		[Given(@"I am an agent without access to text requests")]
		public void GivenIAmAnAgentWithoutAccessToTextRequests()
		{
			UserFactory.User().Setup(new AgentWithoutTextRequestsAccess());
		}

		[Given(@"I am an agent with no access to team schedule")]
		public void GivenIAmAnAgentWithNoAccessToTeamSchedule()
		{
			UserFactory.User().Setup(new AgentWithoutTeamScheduleAccess());
		}

		[Given(@"I have several virtual schedule periods")]
		public void GivenIHaveSeveralVirtualSchedulePeriods()
		{
			UserFactory.User().ReplaceSetupByType(new SchedulePeriod(2));
		}

		[Given(@"I do not have a virtual schedule period")]
		public void GivenIDoNotHaveAVirtualSchedulePeriod()
		{
			UserFactory.User().Setup(new DoNotHaveVirtualSchedulePeriods());
		}

		[Given(@"I have (existing|a) student availability")]
		public void GivenIHaveExistingStudentAvailability(string aOrExisting)
		{
			UserFactory.User().Setup(new StudentAvailability());
		}

		[Given(@"I have (existing|a) shift category preference")]
		public void GivenIHaveExistingShiftCategoryPreference(string aOrExisting)
		{
			UserFactory.User().Setup(new ShiftCategoryPreference());
		}

		[Given(@"I have (existing|a) day off preference")]
		public void GivenIHaveExistingDayOffPreference(string aOrExisting)
		{
			UserFactory.User().Setup(new DayOffPreference());
		}

		[Given(@"I have (existing|a) absence preference")]
		public void GivenIHaveExistingAbsencePreference(string aOrExisting)
		{
			UserFactory.User().Setup(new AbsencePreference());
		}

		[Given(@"I have (existing|a) preference")]
		[Given(@"I have (existing|a) preference today")]
		public void GivenIHaveExistingPreference(string aOrExisting)
		{
			UserFactory.User().Setup(new ExistingPreferenceToday());
		}

		[Given(@"I have a preference with end time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithEndTimeLimitationBetweenAnd(string earliest, string latest)
		{
			UserFactory.User().Setup(new ExistingExtendedPreferenceToday(null, null, earliest, latest, null, null));
		}

		[Given(@"I have a preference with start time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithStartTimeLimitationBetweenAnd(string earliest, string latest)
		{
			UserFactory.User().Setup(new ExistingExtendedPreferenceToday(earliest, latest, null, null, null, null));
		}

		[Given(@"I have a preference with work time limitation between (.*) and (.*)")]
		public void GivenIHaveAPreferenceWithWorkTimeLimitationBetweenAnd(string shortest, string longest)
		{
			UserFactory.User().Setup(new ExistingExtendedPreferenceToday(null, null, null, null, shortest, longest));
		}

		[Given(@"My schedule is published")]
		public void GivenMyScheduleIsPublished()
		{
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

		[Given(@"I have a workflow control set")]
		public void GivenIHaveAWorkflowControlSet()
		{
			UserFactory.User().Setup(new ExistingWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open availability periods")]
		[Given(@"I am in an open student availability period")]
		public void GivenIHaveAWorkflowControlSetWithOpenAvailabilityPeriods()
		{
			UserFactory.User().Setup(new StudentAvailabilityOpenWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed student availability periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedStudentAvailabilityPeriods()
		{
			UserFactory.User().Setup(new StudentAvailabilityClosedWorkflowControlSet());
		}

		[Given(@"I do not have a workflow control set")]
		public void GivenIDoNotHaveAWorkflowControlSet()
		{
			UserFactory.User().Setup(new NoWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with student availability periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithStudentAvailabilityPeriodsOpenNextMonth()
		{
			UserFactory.User().Setup(new StudentAvailabilityOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with open standard preference period")]
		public void GivenIHaveAWorkflowControlSetWithOpenStandardPreferencePeriod()
		{
			UserFactory.User().Setup(new PreferenceOpenWorkflowControlSet());
		}

		[Given(@"I have an open workflow control set with an allowed standard preference")]
		public void GivenIHaveAnOpenWorkflowControlSetWithAnAllowedStandardPreference()
		{
			UserFactory.User().Setup(new PreferenceOpenWithAllowedPreferencesWorkflowControlSet());
		}

		[Given(@"I have existing standard preference")]
		public void GivenIHaveExistingStandardPreference()
		{
			UserFactory.User().Setup(new StandardPreference());
		}

		[Given(@"I have 2 existing standard preference")]
		public void GivenIHave2ExistingStandardPreference()
		{
			UserFactory.User().Setup(new StandardPreference());
			UserFactory.User().Setup(new AnotherStandardPreference());
		}

		[Given(@"I have a workflow control set with preference periods open next month")]
		public void GivenIHaveAWorkflowControlSetWithPreferencePeriodsOpenNextMonth()
		{
			UserFactory.User().Setup(new PreferenceOpenNextMonthWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with closed preference periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedPreferencePeriods()
		{
			UserFactory.User().Setup(new PreferenceClosedWorkflowControlSet());
		}

		[Given(@"My schedule is not published")]
		public void GivenMyScheduleIsNotPublished()
		{
			UserFactory.User().Setup(new ScheduleIsNotPublished());
		}

		[Given(@"I have shifts scheduled for two weeks")]
		public void GivenIHaveAScheduleForTwoWeeks()
		{
			UserFactory.User().Setup(new ShiftsForTwoWeeks());
		}

		[Given(@"I have a shift today")]
		public void GivenIHaveAShiftToday()
		{
			UserFactory.User().Setup(new ShiftToday());
		}


		[Given(@"I have a dayoff today")]
		public void GivenIHaveADayoffToday()
		{
			UserFactory.User().Setup(new DayOffToday());
		}

		[Given(@"I have a contract dayoff today")]
		public void GivenIHaveAContractDayoffToday()
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I have a full-day absence today")]
		public void GivenIHaveAFull_DayAbsenceToday()
		{
			UserFactory.User().Setup(new AbsenceToday());
		}


		[Given(@"My schedule is published until wednesday")]
		public void GivenMyScheduleIsPublishedUntilWednesday()
		{
			UserFactory.User().Setup(new ScheduleIsPublishedUntilWednesday());
		}

		[Given(@"I have a night shift starting on monday")]
		public void GivenIHaveANightShiftOnStartingMonday()
		{
			UserFactory.User().Setup(new NightShiftOnMonday());
		}

		[Given(@"I have a shift on thursday")]
		public void GivenIHaveAShiftOnThursday()
		{
			UserFactory.User().Setup(new ShiftOnThursday());
		}

		[Given(@"I have a shift from (.*) to (.*)")]
		public void GivenIHaveAShiftFrom756To1700(string from, string to)
		{
			UserFactory.User().Setup(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to)));
		}

		[Given(@"I have an activity from (.*) to (.*)")]
		public void GivenIHaveAnActivityFromTo(string from, string to)
		{
			UserFactory.User().Setup(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to), false));
		}

		[Given(@"I have a absence from (.*) to (.*)")]
		public void GivenIHaveAAbsenceFromTo(string from, string to)
		{
			UserFactory.User().Setup(new AbsenceToday());
		}



		[Given(@"I have a meeting scheduled on thursday")]
		public void GivenIHaveAMeetingScheduledOnThursday()
		{
			UserFactory.User().Setup(new MeetingOnThursday());
		}

		[Given(@"I have a public note on tuesday")]
		public void GivenIHaveAPublicNoteOnTuesday()
		{
			UserFactory.User().Setup(new PublicNoteOnWednesday());
		}

		[Given(@"I am swedish")]
		public void GivenIAmSwedish()
		{
			UserFactory.User().SetupCulture(new SwedishCulture());
		}

		[Given(@"I am american")]
		public void GivenIAmUS()
		{
			UserFactory.User().SetupCulture(new USCulture());
		}

		[Given(@"I am located in hawaii")]
		public void GivenIAmLocatedInAmerica()
		{
			UserFactory.User().SetupCulture(new HawaiiTimeZone());
		}

		[Given(@"I have an existing text request")]
		public void GivenIHaveAnExistingTextRequest()
		{
			UserFactory.User().Setup(new ExistingTextRequest());
		}

		[Given(@"I have no existing text requests")]
		public void GivenIHaveNoExistingTextRequests()
		{

		}

		[Given(@"I have an approved text request")]
		public void GivenIHaveAnApprovedTextRequest()
		{
			UserFactory.User().Setup(new ExistingApprovedTextRequest());
		}

		[Given(@"I have a denied text request")]
		public void GivenIHaveAnDeniedTextRequest()
		{
			UserFactory.User().Setup(new ExistingDeniedTextRequest());
		}
		[Given(@"I have more than one page of requests")]
		public void GivenIHaveMoreThanOnePageOfRequests()
		{
			UserFactory.User().Setup(new MoreThanOnePageOfRequests());
		}

		[Given(@"I have 2 existing request changed on different times")]
		public void GivenIHave2ExistingRequestChangedOnDifferentTimes()
		{
			UserFactory.User().Setup(new TwoExistingTextRequestChangedOnDifferentTimes());
		}

		[Given(@"I have an existing text request spanning over 2 days")]
		public void GivenIHaveAnExistingTextRequestSpanningOver2Days()
		{
			UserFactory.User().Setup(new ExistingTextRequestOver2Days());
		}

		[Given(@"the site has another team")]
		public void GivenTheSiteHasAnotherTeam()
		{
			UserFactory.User().Setup(new AnotherTeam());
		}

		[Given(@"the other site has 2 teams")]
		public void GivenTheOtherSiteHas2Teams()
		{
			UserFactory.User().Setup(new AnotherSitesTeam());
			UserFactory.User().Setup(new AnotherSitesSecondTeam());
		}
	
		[Given(@"I belong to another site's team tomorrow")]
		public void GivenIBelongToAnotherSiteSTeamTomorrow()
		{
			var team = new AnotherSitesTeam();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam, DateTime.Today.AddDays(1)));
		}

		[Given(@"I have a shift bag with start times (.*) to (.*) and end times (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithStartTimesToAndEndTimesTo(int earliestStart, int latestStart, int earliestEnd, int latestEnd)
		{
			UserFactory.User().Setup(new RuleSetBag(earliestStart, latestStart, earliestEnd, latestEnd));
		}

		[Given(@"I have a shift bag")]
		public void GivenIHaveAShiftBag()
		{
			UserFactory.User().Setup(new RuleSetBag(8, 10, 16, 18));
		}


		[Given(@"I am an agent in a team that leaves tomorrow")]
		public void GivenIAmAnAgentThatLeavesTomorrow()
		{
			UserFactory.User().Setup(new AgentThatLeavesTomorrow());
			var team = new Team();
			UserFactory.User().Setup(team);
			UserFactory.User().Setup(new SchedulePeriod());
			UserFactory.User().Setup(new PersonPeriod(team.TheTeam));
			UserFactory.User().Setup(new ScheduleIsPublished());
		}

	}
}
