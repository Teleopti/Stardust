using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Specific;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class TeamScheduleStepDefinition
	{
		[When(@"I select the other team in the team picker")]
		public void WhenIChooseTheOtherTeamInTheTeamPicker()
		{
			var team = DataMaker.Data().UserData<AnotherTeam>().TheTeam;
			var site = GlobalDataMaker.Data().Data<CommonSite>().Site.Description.Name;
			var id = team.Id.ToString();
			var text = site + "/" + team.Description.Name;
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
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
				DataMaker.Data().MePerson.Name));
		}

		[Then(@"I should see my schedule in team schedule with")]
		public void WhenIShouldSeeMyScheduleInTeamScheduleWith(Table table)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, table.Rows[0][1]));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, table.Rows[1][1]));
		}

		[Then(@"I should see my colleague's schedule")]
		public void ThenIShouldSeeMyColleagueSSchedule()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
				DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name));
		}

		[Then(@"I should see '(.*)' schedule in team schedule with")]
		public void ThenIShouldSeeScheduleInTeamScheduleWith(string name, Table table)
		{
			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(
					".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']", name,
					table.Rows[0][1]));
			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(
					".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']", name,
					table.Rows[1][1]));
		}

		[Then(@"I should see my colleague's absence")]
		public void ThenIShouldMyColleagueSAbsence()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
				DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name));
		}

		[Then(@"I should see the next day")]
		[Then(@"I should see tomorrow")]
		public void ThenIShouldSeeTheNextDay()
		{
			AssertShowingDay(new DateOnly(2014,5,3));
		}

		[Then(@"I should see the previous day")]
		public void ThenIShouldSeeThePreviousDay()
		{
			AssertShowingDay(new DateOnly(2014, 5, 1));
		}

		private void AssertShowingDay(DateOnly date)
		{
			Browser.Interactions.AssertUrlContains(string.Format("{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
		}

		[Then(@"I should see the absence with color (.*)")]
		public void ThenIShouldNotSeeTheAbsenceSColor(string colorName)
		{
			var color = Color.FromName(colorName);
			var colorAsString = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);

			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
					DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name),
				string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[style*='{1}']",
					DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name, colorAsString));
		}

		[Then(@"I should see my colleague's day off")]
		public void ThenIShouldSeeMyColleagueSDayOff()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .dayoff",
				DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name));
		}

		[Then(@"The time line should span from (.*) to (.*)")]
		public void ThenTheTimeLineShouldSpanFromAToB(string from, string to)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(".teamschedule-timeline-label:first", from);
			Browser.Interactions.AssertFirstContainsUsingJQuery(".teamschedule-timeline-label:last", to);
		}

		[Then(@"I should not see my colleagues schedule")]
		public void ThenIShouldNotSeeMyColleaguesSchedule()
		{
			AssertAgentIsNotDisplayed(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should not see '(.*)' schedule")]
		public void ThenIShouldNotSeeSchedule(string name)
		{
			AssertAgentIsNotDisplayed(name);
		}

		[Then(@"I should not see the other colleague's schedule")]
		public void ThenIShouldNotSeeTheOtherColleagueSSchedule()
		{
			AssertAgentIsNotDisplayed(DataMaker.Person(ColleagueStepDefinitions.OtherTeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should see my colleague before myself")]
		public void ThenIShouldSeeMyColleagueBeforeMyself()
		{
			ThenIShouldSeeBeforeMyself(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
		}

		[Then(@"I should see '(.*)' before myself")]
		public void ThenIShouldSeeBeforeMyself(string name)
		{
			Browser.Interactions.AssertFirstContainsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:nth(0)"), name);
			Browser.Interactions.AssertFirstContainsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:nth(1)"), DataMaker.Data().MePerson.Name.ToString());
		}


		[Then(@"I should see myself without schedule")]
		public void ThenIShouldSeeMyselfWithoutSchedule()
		{
			var name = DataMaker.Data().MePerson.Name;
			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule",
					name), string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
						name));
		}

		[Then(@"I should see my colleague without schedule")]
		public void ThenIShouldSeeMyColleagueWithoutSchedule()
		{
			var name = DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name;
			Browser.Interactions.AssertNotExistsUsingJQuery(
				string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule",
					name), string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer",
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
			var site = GlobalDataMaker.Data().Data<CommonSite>().Site.Description.Name;
			var myTeam = site + "/" + DataMaker.Data().UserData<Team>().TheTeam.Description.Name;
			var otherTeam = site + "/" + DataMaker.Data().UserData<AnotherTeam>().TheTeam.Description.Name;

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
			if (DataMaker.PersonExists(ColleagueStepDefinitions.TeamColleagueName))
				AssertAgentIsDisplayed(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			else
				AssertAgentIsDisplayed(DataMaker.Person(ColleagueStepDefinitions.OtherTeamColleagueName).Person.Name.ToString());
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
		public void ThenTheLayerSStartTimeAttibuteValueShouldBe(string startTime)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, startTime));
		}

		[Then(@"The layer's end time attibute value should be (.*)")]
		public void ThenTheLayerSEndTimeAttibuteValueShouldBe(string endTime)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-text*='{1}']",
				DataMaker.Data().MePerson.Name, endTime));
		}

		[Then(@"The layer's activity name attibute value should be (.*)")]
		public void ThenTheLayerSActivityNameAttibuteValueShouldBe(string activityName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".teamschedule-agent-name-without-badge:contains('{0}') + .teamschedule-agent-schedule .layer[tooltip-title*='{1}']",
				DataMaker.Data().MePerson.Name, activityName));
		}

		[Then(@"I should see the team-picker with the other site's team")]
		public void ThenIShouldSeeTheTeam_PickerWithTheOtherSiteSTeam()
		{
			var teamId = DataMaker.Data().UserData<AnotherSitesTeam>().TheTeam.Id;

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
			var theOtherSitesTeam = DataMaker.Data().UserData<AnotherSitesTeam>().TheTeam.Description.Name;
			Browser.Interactions.AssertAnyContains(".select2-chosen", theOtherSitesTeam);
		}

		[Then(@"the team-picker should have my team selected")]
		public void ThenTheTeam_PickerShouldHaveMyTeamSelected()
		{
			var myTeam = DataMaker.Data().UserData<Team>().TheTeam.Id.Value.ToString();
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
			Browser.Interactions.Click(".hidden-sm .initialize-shift-trade");
		}

		[Then(@"I should not be able to initialize a shift trade")]
		public void ThenIShouldNotBeAbleToInitializeAShiftTrade()
		{
			Browser.Interactions.AssertNotExists(".navbar", ".initialize-shift-trade");
		}

		private static void AssertAgentIsDisplayed(string name)
		{
			Browser.Interactions.AssertAnyContains(".teamschedule-agent-name-without-badge", name);
		}

		private static void AssertAgentIsNotDisplayed(string name)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".teamschedule-agent-name-without-badge", string.Format(".teamschedule-agent-name-without-badge:contains('{0}')", name));
		}
	}
}
