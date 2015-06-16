﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using PersonAbsenceConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonAbsenceConfigurable;
using PersonDayOffConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonDayOffConfigurable;
using SiteConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.SiteConfigurable;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Scope(Feature = "Team schedule With Paging And Filter Feature")]
	[Binding]
	public class TeamscheduleWithPagingAndFilterFeatureStepDefinition
	{
		private static string TeamColleagueName = "Team Colleague";
		private static string OtherTeamColleagueName = "NonTeam Colleague";

		private class TheOtherSite : SiteConfigurable
		{
			public TheOtherSite()
			{
				BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name;
				Name = "The other site";
			}
		}

		private static ISite GetTheOtherSite()
		{
			if (!DataMaker.Data().HasSetup<TheOtherSite>())
				DataMaker.Data().Apply(new TheOtherSite());
			return DataMaker.Data().UserData<TheOtherSite>().Site;
		}

		private class TheOtherTeam : GeneratedTeam
		{
			public TheOtherTeam() : base(DefaultSite.Get()) { }
		}

		private static ITeam GetTheOtherTeam()
		{
			if (!DataMaker.Data().HasSetup<TheOtherTeam>())
				DataMaker.Data().Apply(new TheOtherTeam());
			return DataMaker.Data().UserData<TheOtherTeam>().TheTeam;
		}

		private class TheOtherSitesFirstTeam : GeneratedTeam
		{
			public TheOtherSitesFirstTeam() : base(GetTheOtherSite()) { }
		}

		private static ITeam GetTheOtherSitesFirstTeam()
		{
			if (!DataMaker.Data().HasSetup<TheOtherSitesFirstTeam>())
				DataMaker.Data().Apply(new TheOtherSitesFirstTeam());
			return DataMaker.Data().UserData<TheOtherSitesFirstTeam>().TheTeam;
		}

		private class TheOtherSitesSecondTeam : GeneratedTeam
		{
			public TheOtherSitesSecondTeam() : base(GetTheOtherSite()) { }
		}

		private static ITeam GetTheOtherSitesSecondTeam()
		{
			if (!DataMaker.Data().HasSetup<TheOtherSitesSecondTeam>())
				DataMaker.Data().Apply(new TheOtherSitesSecondTeam());
			return DataMaker.Data().UserData<TheOtherSitesSecondTeam>().TheTeam;
		}

		[Given(@"the site has another team")]
		public void GivenTheSiteHasAnotherTeam()
		{
			DefaultSite.Get();
			DefaultTeam.Get();
			GetTheOtherTeam();
		}

		[Given(@"the other site has 2 teams")]
		public void GivenTheOtherSiteHas2Teams()
		{
			GetTheOtherSite();
			GetTheOtherSitesFirstTeam();
			GetTheOtherSitesSecondTeam();
		}

		[Given(@"I belong to another site's team on '(.*)'")]
		public void GivenIBelongToAnotherSiteSTeamTomorrow(DateTime date)
		{
			GetTheOtherSite();
			var team = GetTheOtherSitesFirstTeam();
			DataMaker.Data().Apply(new PersonPeriod(team, date));
		}

		[Given(@"I have a colleague '(.*)'")]
		public void GivenIHaveAColleague(string colleagueName)
		{
			DataMaker.Person(colleagueName).Apply(new Agent());
			DataMaker.Person(colleagueName).Apply(new SchedulePeriod());
			DataMaker.Person(colleagueName).Apply(new PersonPeriod(DefaultTeam.Get()));

			if (colleagueName.Contains("Unpublished"))
			{		
				DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Unpublished2", SchedulePublishedToDate = "1990-01-01" , PreferencePeriodIsClosed = true});
				DataMaker.Person(colleagueName).Apply(new WorkflowControlSetForUser { Name = "Unpublished2" });
			}
			else { 
				DataMaker.Data()
					.Apply(new WorkflowControlSetConfigurable {Name = "Published2", SchedulePublishedToDate = "2030-12-01"});
				DataMaker.Person(colleagueName).Apply(new WorkflowControlSetForUser {Name = "Published2"});
			}			
		}


		[Given(@"I have a colleague")]
		public void GivenIHaveAColleague()
		{
			DataMaker.Person(TeamColleagueName).Apply(new Agent());
			DataMaker.Person(TeamColleagueName).Apply(new SchedulePeriod());
			DataMaker.Person(TeamColleagueName).Apply(new PersonPeriod(DefaultTeam.Get()));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published2", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Person(TeamColleagueName).Apply(new WorkflowControlSetForUser { Name = "Published2" });
		}

		[Given(@"I have a colleague in another team")]
		public void GivenIHaveAColleagueInAnotherTeam()
		{
			DataMaker.Person(OtherTeamColleagueName).Apply(new Agent());
			DataMaker.Person(OtherTeamColleagueName).Apply(new SchedulePeriod());
			DataMaker.Person(OtherTeamColleagueName).Apply(new PersonPeriod(GetTheOtherTeam()));
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published3", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Person(OtherTeamColleagueName).Apply(new WorkflowControlSetForUser { Name = "Published3" });
		}

		[Given(@"My colleague has an assigned shift with")]
		public void GivenMyColleagueHasAShiftWith(Table table)
		{
			DataMaker.ApplyFromTable<AssignedShift>(TeamColleagueName, table);
		}

		[Given(@"My colleague has an assigned dayoff with")]
		public void GivenMyColleagueHasADayoffWith(Table table)
		{
			var personDayOff = table.CreateInstance<PersonDayOffConfigurable>();
			DataMaker.Person(TeamColleagueName).Apply(personDayOff);
		}

		[Given(@"My colleague has an assigned absence with")]
		[Given(@"My colleague has an assigned full-day absence with")]
		public void GivenMyColleagueHasAnAbsenceWith(Table table)
		{
			DataMaker.ApplyFromTable<PersonAbsenceConfigurable>(TeamColleagueName, table);
		}

		[Given(@"The colleague in the other team has an assigned shift with")]
		public void GivenTheColleagueInTheOtherTeamHasAShiftWith(Table table)
		{
			DataMaker.ApplyFromTable<AssignedShift>(OtherTeamColleagueName, table);
		}

		[Given(@"My colleague has a confidential absence with")]
		public void GivenMyColleagueHasAnConfidentialAbsence(Table table)
		{
			DataMaker.ApplyFromTable<ShiftWithConfidentialAbsence>(TeamColleagueName, table);
		}

		[When(@"I select the other team in the team picker")]
		public void WhenIChooseTheOtherTeamInTheTeamPicker()
		{
			var team = GetTheOtherTeam();
			var id = team.Id.ToString();
			var text = team.Site.Description.Name + "/" + team.Description.Name;
			Select2Box.SelectItemByIdAndText("Team-Picker", id, text);
			Browser.Interactions.AssertExists(".input-group-btn button:nth-of-type(2)");
		}

		[When(@"I select '(.*)' in the team picker")]
		public void WhenISelectInTheTeamPicker(string optionText)
		{
			IOpenTheTeamPicker();

			Select2Box.SelectItemByText("Team-Picker", optionText);
		}

		[When(@"I select something in the team picker")]
		public void WhenISelectSomethingInTheTeamPicker()
		{
			IOpenTheTeamPicker();

			Select2Box.SelectFirstOption("Team-Picker");
		}

		[Then(@"I should see the team schedule tab")]
		public void ThenIShouldSeeTheTeamScheduleTab()
		{
			Browser.Interactions.AssertExists("a[href*='#TeamScheduleTab']");
		}

		[Then(@"I should not see the team schedule tab")]
		public void ThenIShouldNotSeeTheTeamScheduleTab()
		{
			Browser.Interactions.AssertNotExists(".navbar-nav", "a[href*='#TeamScheduleTab']");
		}

		[Then(@"I should see my schedule")]
		public void ThenIShouldSeeMySchedule()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container",
				DataMaker.Data().MePerson.Name));
		}

		[Then(@"I should see my schedule in team schedule with")]
		public void WhenIShouldSeeMyScheduleInTeamScheduleWith(Table table)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, table.Rows[0][1]));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, table.Rows[1][1]));
		}

		[Then(@"I should see my colleague's schedule")]
		public void ThenIShouldSeeMyColleagueSSchedule()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container", DataMaker.Person(TeamColleagueName).Person.Name));
		}

		[Then(@"I should see '(.*)' schedule in team schedule with")]
		public void ThenIShouldSeeScheduleInTeamScheduleWith(string name, Table table)
		{
			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(
					".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container[tooltip-text*='{1}']", name,
					table.Rows[0][1]));
			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(
					".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container[tooltip-text*='{1}']", name,
					table.Rows[1][1]));
		}

		[Then(@"I should see my colleague's absence")]
		public void ThenIShouldMyColleagueSAbsence()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container", DataMaker.Person(TeamColleagueName).Person.Name));
		}

		[Then(@"I should see the next day")]
		[Then(@"I should see tomorrow")]
		public void ThenIShouldSeeTheNextDay()
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#ScheduleDatePicker .date-input-style","2014-05-03");
		}

		[Then(@"I should see the previous day")]
		public void ThenIShouldSeeThePreviousDay()
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#ScheduleDatePicker .date-input-style", "2014-05-01");
		}


		[Then(@"I should see the absence with color (.*)")]
		public void ThenIShouldNotSeeTheAbsenceSColor(string colorName)
		{
			var color = Color.FromName(colorName);
			var colorAsString = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);

			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container",
					DataMaker.Person(TeamColleagueName).Person.Name),
				string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container[style*='{1}']",
					DataMaker.Person(TeamColleagueName).Person.Name, colorAsString));
		}

		[Then(@"I should see my colleague's day off")]
		public void ThenIShouldSeeMyColleagueSDayOff()	   
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container",
				DataMaker.Person(TeamColleagueName).Person.Name));
		}

		[Then(@"The time line should span from (.*) to (.*)")]
		public void ThenTheTimeLineShouldSpanFromAToB(string from, string to)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-timeline", from);
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-timeline", to);
		}

		[Then(@"I should not see my colleagues schedule")]
		public void ThenIShouldNotSeeMyColleaguesSchedule()
		{
			AssertAgentIsNotDisplayed(DataMaker.Person(TeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should not see '(.*)' schedule")]
		public void ThenIShouldNotSeeSchedule(string name)
		{
			AssertAgentIsNotDisplayed(name);
		}

		[Then(@"I should not see the other colleague's schedule")]
		public void ThenIShouldNotSeeTheOtherColleagueSSchedule()
		{
			AssertAgentIsNotDisplayed(DataMaker.Person(OtherTeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should see my colleague before myself")]
		public void ThenIShouldSeeMyColleagueBeforeMyself()
		{
			ThenIShouldSeeBeforeMyself(DataMaker.Person(TeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should see '(.*)' before myself")]
		public void ThenIShouldSeeBeforeMyself(string name)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-agent-name:nth(1)", name);
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-agent-name:nth(2)", DataMaker.Data().MePerson.Name.ToString());
		}

		[Then(@"I should see myself before my colleague")]
		public void ThenIShouldSeeMyselfBeforeMyColleague()
		{
			Browser.Interactions.AssertExists("#existsWhenLoadingFinishes");
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-agent-name:nth(1)", DataMaker.Data().MePerson.Name.ToString());
			Browser.Interactions.AssertFirstContainsUsingJQuery(".shift-trade-agent-name:nth(2)", DataMaker.Person(TeamColleagueName).Person.Name.ToString());
		}


		[Then(@"I should see myself without schedule")]
		public void ThenIShouldSeeMyselfWithoutSchedule()
		{
			var name = DataMaker.Data().MePerson.Name;
			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule",
					name), string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container",
						name));
		}

		[Then(@"I should see my colleague without schedule")]
		public void ThenIShouldSeeMyColleagueWithoutSchedule()
		{
			var name = DataMaker.Person(TeamColleagueName).Person.Name;
			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule",
					name), string.Format(".shift-trade-agent-name:contains('{0}') + .shift-trade-possible-trade-schedule .shift-trade-layer-container",
						name));
		}

		[When(@"I open the team-picker")]
		public void IOpenTheTeamPicker()
		{
			Select2Box.Open("Team-Picker");
		}

		[Then(@"I should see the team-picker with both teams")]
		public void ThenIShouldSeeTheTeam_PickerWithBothTeams()
		{
			var team1 = DefaultTeam.Get();
			var team2 = GetTheOtherTeam();

			var myTeam = team1.Site.Description.Name + "/" + team1.Description.Name;
			var otherTeam = team2.Site.Description.Name + "/" + team2.Description.Name;

			Select2Box.AssertOptionExist("Team-Picker", myTeam);
			Select2Box.AssertOptionExist("Team-Picker", otherTeam);
		}

		[Then(@"I should see available (team|group) options")]
		public void ThenIShouldSeeAvailableGroupOptions(string teamGroup, Table table)
		{
			var options = table.CreateSet<SingleValue>();

			foreach (var option in options)
			{
				Select2Box.AssertOptionExist("Team-Picker", option.Value);
			}
		}

		private class SingleValue
		{
			public string Value { get; set; }
		}

		[Then(@"the teams should be sorted alphabetically")]
		public void ThenTheTeamsShouldBeSortedAlphabetically()
		{
			var firstTeam = Select2Box.FirstOptionText;
			var lastTeam = Select2Box.LastOptionText;
			var teams = new List<string> { lastTeam, firstTeam }.OrderBy(t => t).ToArray();

			teams.First().Should().Be.EqualTo(firstTeam);
			teams.Last().Should().Be.EqualTo(lastTeam);
		}

		[Then(@"I should see my colleague")]
		public void ThenIShouldSeeMyColleague()
		{
			// refact hack to see if which colleague has been created in this scenario
			if (DataMaker.PersonExists(TeamColleagueName))
				AssertAgentIsDisplayed(DataMaker.Person(TeamColleagueName).Person.Name.ToString());
			else
				AssertAgentIsDisplayed(DataMaker.Person(OtherTeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should see colleague '(.*)'")]
		public void ThenIShouldSeeColleague(string personName)
		{
			AssertAgentIsDisplayed(personName);
		}


		[Then(@"I should not see myself")]
		public void ThenIShouldNotSeeMySchedule()
		{
			AssertAgentIsNotDisplayed(DataMaker.Data().MePerson.Name.ToString());
		}

		[Then(@"The layer's start time attibute value should be (.*)")]
		public void ThenTheLayerSStartTimeAttibuteValueShouldBe(string Time)
		{			
			Browser.Interactions.AssertExistsUsingJQuery(".shift-trade-agent-name:contains('I')");
			var mouseEnterScript = "$(\".shift-trade-agent-name:contains('I') + .shift-trade-possible-trade-schedule .shift-trade-layer-container .shift-trade-layer\").first().mouseenter(); ";

			Browser.Interactions.Javascript(mouseEnterScript);
			var tooltipScript =
				"return $(\".shift-trade-agent-name:contains('I') + .shift-trade-possible-trade-schedule .shift-trade-layer-container .tooltip-inner\").text();";

			Browser.Interactions.AssertJavascriptResultContains(tooltipScript, Time);
		}

		[Then(@"I should see the team-picker with the other site's team")]
		public void ThenIShouldSeeTheTeam_PickerWithTheOtherSiteSTeam()
		{
			var teamId = GetTheOtherSitesFirstTeam().Id;

			var name = DataMaker.Data().MePerson.Name.ToString();
			AssertAgentIsDisplayed(name);

			Select2Box.AssertSelectedOptionValue("Team-Picker", teamId.ToString());
		}

		[Then(@"The team picker should have '(.*)' selected")]
		public void ThenTheTeamPickerShouldHaveSelected(string optionSelected)
		{
			Select2Box.AssertSelectedOptionText("Team-Picker", optionSelected);
		}

		[Then(@"I should see the other site's team")]
		public void ThenIShouldSeeTheOtherSiteSTeam()
		{
			var theOtherSitesTeam = GetTheOtherSitesFirstTeam().Description.Name;
			Browser.Interactions.AssertAnyContains(".select2-chosen", theOtherSitesTeam);
		}

		[Then(@"the team-picker should have my team selected")]
		public void ThenTheTeam_PickerShouldHaveMyTeamSelected()
		{
			var myTeam = DefaultTeam.Get().Id.Value.ToString();
			Select2Box.AssertSelectedOptionValue("Team-Picker", myTeam);
		}

		[Then(@"I should not see the team-picker")]
		public void ThenIShouldNotSeeTheTeam_Picker()
		{
			Browser.Interactions.AssertNotExists(".navbar-nav", "#Team-Picker");
		}

		[Then(@"I should see the date-picker")]
		public void ThenIShouldSeeTheDatePicker()
		{
			Browser.Interactions.AssertExists("#ScheduleDatePicker");
		}

		[Then(@"I should see the team-picker")]
		public void ThenIShouldSeeTheTeam_PickerWithTwoTeams()
		{
			Browser.Interactions.AssertExists("#Team-Picker");
		}

		[When(@"I initialize a shift trade")]
		public void WhenIInitializeAShiftTrade()
		{
			Browser.Interactions.Click(".submenu .initialize-shift-trade");
		}

		[Then(@"I should not be able to initialize a shift trade")]
		public void ThenIShouldNotBeAbleToInitializeAShiftTrade()
		{
			Browser.Interactions.AssertNotExists(".navbar", ".initialize-shift-trade");
		}

		[When(@"I click the previous day button in datepicker")]
		public void WhenIClickThePreviousDayButtonInDatepicker()
		{
			Browser.Interactions.Click("button.previous-date");
		}

		[When(@"I click the next day button in datepicker")]
		public void WhenIClickTheNextDayButtonInDatepicker()
		{
			Browser.Interactions.Click("button.next-date");
		}

		[When(@"I type '(.*)' in the name search box")]
		public void WhenITypeInTheNameSearchBox(string partialName)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".name-search input.form-control", partialName);
		}


		[Then(@"I click sort by end time asc at end time drop down button")]
		public void ThenIClickSortByEndTimeAscAtEndTimeDropDownButton()
		{
			Browser.Interactions.AssertExists("#existsWhenLoadingFinishes");
			Browser.Interactions.ClickContaining(".row .filter-time-dropdown-button .dropdown-toggle", "Sluttid");
			Browser.Interactions.Click(".filter-time-dropdown-button .end-time-sort-order .glyphicon-arrow-up");
		}

		[Then(@"Agent '(.*)' schedule should be empty")]
		public void ThenAgentScheduleShouldBeEmpty(string p0)
		{
			Browser.Interactions.AssertAnyContains(".shift-trade-agent-name", p0);

			var tooltipScript =
				"return $(\".shift-trade-agent-name:contains('" + p0 +"') + .shift-trade-possible-trade-schedule .shift-trade-layer-container\").length ;";

			Browser.Interactions.AssertJavascriptResultContains(tooltipScript, "0");
		}


		private static void AssertAgentIsDisplayed(string name)
		{
			Browser.Interactions.AssertAnyContains(".shift-trade-agent-name", name);
		}

		private static void AssertAgentIsNotDisplayed(string name)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".shift-trade-agent-name",
				string.Format(".shift-trade-agent-name:contains('{0}')",
					name));
		}
	}
}
