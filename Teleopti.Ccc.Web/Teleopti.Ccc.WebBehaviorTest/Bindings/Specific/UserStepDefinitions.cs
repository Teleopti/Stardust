using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class UserStepDefinitions
	{
		[Given(@"I am an agent")]
		public void GivenIAmAnAgent()
		{
			DataMaker.Data().Apply(new Agent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod());
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable{Name = "Published", SchedulePublishedToDate = "2030-12-01"});
			DataMaker.Data().Apply(new WorkflowControlSetForUser{Name = "Published"});
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
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}

		[Given(@"I am an agent in no team with access to my team")]
		public void GivenIAmAnAgentInNoTeamWithAccessToMyTeam()
		{
			DataMaker.Data().Apply(new Agent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}
		
		[Given(@"I am an agent in a team with access to the whole site")]
		public void GivenIAmAnAgentInATeamWithAccessToTheWholeSite()
		{
			DataMaker.Data().Apply(new AgentWithSiteAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
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
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}

		[Given(@"I am an agent in a team without access to shift trade requests")]
		public void GivenIAmAnAgentInATeamWithoutAccessToShiftTradeRequests()
		{
			DataMaker.Data().Apply(new AgentWithoutRequestsAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}

		[Given(@"I am an agent in a team with access only to my own data")]
		public void GivenIAmAnAgentWithoutPermissionToSeeMyColleagueSSchedule()
		{
			DataMaker.Data().Apply(new AgentWithoutTeamDataAccess());
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}

		[Given(@"I am a student agent")]
		public void GivenIAmAStudentAgent()
		{
			DataMaker.Data().Apply(new StudentAgent());
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod());
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published Student", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published Student" });
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
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
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

		[Given(@"I have (existing|a) shift category preference with")]
		public void GivenIHaveExistingShiftCategoryPreference(string aOrExisting, Table table)
		{
			DataMaker.ApplyFromTable<ShiftCategoryPreference>(table);
		}

		[Given(@"I have (existing|a) day off preference with")]
		public void GivenIHaveExistingDayOffPreference(string aOrExisting, Table table)
		{
			DataMaker.ApplyFromTable<DayOffPreference>(table);
		}

		[Given(@"I have (existing|a) absence preference with")]
		public void GivenIHaveExistingAbsencePreference(string aOrExisting, Table table)
		{
			DataMaker.ApplyFromTable<AbsencePreference>(table);
		}

		[Given(@"I have a preference with end time limitation between (.*) and (.*) for '(.*)'"), SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithEndTimeLimitationBetweenAnd(int earliest, int latest, DateTime date)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreference(endTimeLimitation) { Date = date.ToShortDateString() });
		}

		[Given(@"I have a preference with start time limitation between (.*) and (.*) for '(.*)'"),SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithStartTimeLimitationBetweenAnd(int earliest, int latest,DateTime date)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreference(startTimeLimitation) { Date = date.ToShortDateString() });
		}

		[Given(@"I have a preference with work time limitation between (.*) and (.*) for '(.*)'"), SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithWorkTimeLimitationBetweenAnd(int shortest, int longest, DateTime date)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(shortest, 0, 0), new TimeSpan(longest, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreference(workTimeLimitation) { Date = date.ToShortDateString() });
		}


		[Given(@"I have preference for the first category for '(.*)'"),SetCulture("sv-SE")]
		public void GivenIHavePreferenceForTheFirstCategoryToday(DateTime date)
		{
			var firstCat = DataMaker.Data().UserData<FirstShiftCategory>();
			var firstCategory = firstCat.ShiftCategory.Description.Name;

			DataMaker.Data().Apply(new ShiftCategoryPreference {ShiftCategory = firstCategory,Date = date.ToShortDateString()});
		}

		[Given(@"I have a preference with (.*) length limitation of 1 hour for '(.*)'"),SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithLunchLengthLimitationOf1HourToday(string lunchActivity, DateTime date)
		{
			var workTimeLimitation = new WorkTimeLimitation(new TimeSpan(1, 0, 0), new TimeSpan(1, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreference(workTimeLimitation){Date = date.ToShortDateString(), LunchActivity = lunchActivity});
		}

		[Given(@"I have a preference with (.*) end time limitation between (.*) and (.*) for '(.*)'"), SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithLunchEndTimeLimitationBetweenAnd(string lunchActivity, int earliest, int latest, DateTime date)
		{
			var endTimeLimitation = new EndTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreference(endTimeLimitation) { Date = date.ToShortDateString(), LunchActivity = lunchActivity });
		}

		[Given(@"I have a preference with (.*) start time limitation between (.*) and (.*) for '(.*)'"), SetCulture("sv-SE")]
		public void GivenIHaveAPreferenceWithLunchStartTimeLimitationBetweenAnd(string lunchActivity, int earliest, int latest, DateTime date)
		{
			var startTimeLimitation = new StartTimeLimitation(new TimeSpan(earliest, 0, 0), new TimeSpan(latest, 0, 0));
			DataMaker.Data().Apply(new ExistingLunchPreference(startTimeLimitation) { Date = date.ToShortDateString(), LunchActivity = lunchActivity });
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

		[Given(@"I have a conflicting preference and availability for '(.*)'"),SetCulture("sv-SE")]
		public void GivenIHaveAConflictingPreferenceAndAvailability(DateTime date)
		{
			var startTimeAvailability = new StartTimeLimitation(new TimeSpan(10, 0, 0), null);
			var startTimePreference = new StartTimeLimitation(null, new TimeSpan(9, 0, 0));
			DataMaker.Data().Apply(new ExistingExtendedPreference(startTimePreference){Date = date.ToShortDateString()});
			DataMaker.Data().Apply(new ExistingAvailability(startTimeAvailability));
		}


		[Given(@"My schedule is published")]
		public void GivenMyScheduleIsPublished()
		{
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published2", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published2" });
		}

		[Given(@"I have a workflow control set")]
		public void GivenIHaveAWorkflowControlSet()
		{
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable{Name = "Simple WCS"});
			DataMaker.Data().Apply(new WorkflowControlSetForUser{Name = "Simple WCS"});
		}

		[Given(@"I have a workflow control set with open availability periods")]
		public void GivenIHaveAWorkflowControlSetWithOpenAvailabilityPeriods()
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Published 100 days, SA open",
					SchedulePublishedToDate = "2030-12-01",
					StudentAvailabilityPeriodIsClosed = false
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published 100 days, SA open" });
		}

		[Given(@"I have a workflow control set with closed student availability periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedStudentAvailabilityPeriods()
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Published 100 days, SA closed",
					SchedulePublishedToDate = "2030-12-01",
					StudentAvailabilityPeriodIsClosed = true
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published 100 days, SA closed" });
		}

		[Given(@"I do not have a workflow control set")]
		public void GivenIDoNotHaveAWorkflowControlSet()
		{
			DataMaker.Data().Apply(new NoWorkflowControlSet());
		}

		[Given(@"I have a workflow control set with student availability periods open from '(.*)' to '(.*)'")]
		public void GivenIHaveAWorkflowControlSetWithStudentAvailabilityPeriodsOpenNextMonth(DateTime start,DateTime end)
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open SA",
					SchedulePublishedToDate = end.AddDays(100).ToShortDateString(),
					StudentAvailabilityPeriodStart = start.ToShortDateString(),
					StudentAvailabilityPeriodEnd = end.ToShortDateString()
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser{Name = "Open SA"});
		}

		[Given(@"I have a workflow control set with open standard preference period")]
		public void GivenIHaveAWorkflowControlSetWithOpenStandardPreferencePeriod()
		{
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable
			{
				PreferencePeriodIsClosed = false,
				SchedulePublishedToDate = "2030-12-01",
				Name = "Preferences open"
			});
			DataMaker.Data().Apply(new WorkflowControlSetForUser{Name = "Preferences open"});
		}

		[Given(@"I have an open workflow control set with an allowed standard preference open from '(.*)' to '(.*)'")]
		public void GivenIHaveAnOpenWorkflowControlSetWithAnAllowedStandardPreference(DateTime start,DateTime end)
		{
			DataMaker.Data().Apply(new DayOffTemplateConfigurable { Name = "Day off" });
			DataMaker.Data().Apply(new AbsenceConfigurable { Name = "Vacation" });
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open",
					SchedulePublishedToDate = end.AddDays(100).ToShortDateString(),
					PreferencePeriodStart = start.ToShortDateString(),
					PreferencePeriodEnd = end.ToShortDateString(),
					AvailableShiftCategory = TestData.ShiftCategory.Description.Name,
					AvailableDayOff = "Day off",
					AvailableAbsence = "Vacation"
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Open" });
		}

		[Given(@"I have existing standard preference with")]
		public void GivenIHaveExistingStandardPreference(Table table)
		{
			DataMaker.ApplyFromTable<StandardPreference>(table);
		}

		[Given(@"I have 2 existing standard preference with")]
		public void GivenIHave2ExistingStandardPreference(Table table)
		{
			DataMaker.ApplyFromTable<StandardPreference>(table);
			DataMaker.ApplyFromTable<AnotherStandardPreference>(table);
		}

		[Given(@"I have a workflow control set with preference periods open from '(.*)' to '(.*)'")]
		public void GivenIHaveAWorkflowControlSetWithPreferencePeriodsOpenNextMonth(DateTime start,DateTime end)
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open",
					SchedulePublishedToDate = end.AddDays(100).ToShortDateString(),
					PreferencePeriodStart = start.ToShortDateString(),
					PreferencePeriodEnd = end.ToShortDateString()
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Open" });
		}

		[Given(@"I have a workflow control set with closed preference periods")]
		public void GivenIHaveAWorkflowControlSetWithClosedPreferencePeriods()
		{
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { PreferencePeriodIsClosed = true, SchedulePublishedToDate = "2030-12-01", Name = "Preferences closed" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Preferences closed" });
		}

		[Given(@"I have an assigned dayoff with")]
		public void GivenIHaveADayoffToday(Table table)
		{
			DataMaker.ApplyFromTable<AssignedDayOff>(table);
		}

		[Given(@"I have an assigned full-day absence with")]
		public void GivenIHaveAFull_DayAbsenceToday(Table table)
		{
			DataMaker.ApplyFromTable<AssignedAbsence>(table);
		}

		[Given(@"I have a full-day contract time absence on '(.*)'"),SetCulture("sv-SE")]
		public void GivenIHaveAFull_DayContractTimeAbsenceToday(DateTime date)
		{
			DataMaker.Data().Apply(new AbsenceInContractTime{Date = date.ToShortDateString()});
		}

		[Given(@"I have a full-day absence with")]
		public void GivenIHaveAFull_DayAbsenceWith(Table table)
		{
			var absence = table.CreateInstance<AssignedAbsence>();
			DataMaker.Data().Apply(absence);
		}

		[Given(@"I have an assigned shift with")]
		public void GivenIHaveAShiftAssigned(Table table)
		{
			DataMaker.ApplyFromTable<AssignedShift>(table);
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
		
		[Given(@"I belong to another site's team on '(.*)'")]
		public void GivenIBelongToAnotherSiteSTeamTomorrow(DateTime date)
		{
			var team = new AnotherSitesTeam();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam, date));
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

		[Given(@"I have a shift bag with one shift (.*) to (.*) and (.*) (.*) to (.*) and one shift (.*) to (.*) and (.*) (.*) to (.*)")]
		public void GivenIHaveAShiftBagWithOneShiftToAndLunchToAndOneShiftToAndLunchTo(int start1, int end1, string lunchActivity1, int lunchStart1, int lunchEnd1, int start2, int end2, string lunchActivity2, int lunchStart2, int lunchEnd2)
		{
			DataMaker.Data().Apply(new RuleSetBagWithTwoShiftsAndLunch(start1, end1, lunchActivity1, lunchStart1, lunchEnd1, start2, end2, lunchActivity2, lunchStart2, lunchEnd2));
		}

		[Given(@"I am an agent in a team that leaves on '(.*)'")]
		public void GivenIAmAnAgentThatLeavesOn(DateTime date)
		{
			DataMaker.Data().Apply(new AgentThatLeaves(date));
			var team = new Team();
			DataMaker.Data().Apply(team);
			DataMaker.Data().Apply(new SchedulePeriod());
			DataMaker.Data().Apply(new PersonPeriod(team.TheTeam));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published" });
		}
	}
}