using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Specific;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
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
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.TeamScheduleLink.Exists, Is.True);
		}

		[Then(@"I should not see the team schedule tab")]
		public void ThenIShouldNotSeeTheTeamScheduleTab()
		{
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.TeamScheduleLink.Exists, Is.False);
		}

		[Then(@"I should see my schedule")]
		public void ThenIShouldSeeMySchedule()
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Data().MePerson.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see my schedule in team schedule with")]
		public void WhenIShouldSeeMyScheduleInTeamScheduleWith(Table table)
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Data().MePerson.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
			var layer = layers.FirstOrDefault();
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[0][1]);
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[1][1]);
		}

		[Then(@"I should see my colleague's schedule")]
		public void ThenIShouldSeeMyColleagueSSchedule()
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see '(.*)' schedule in team schedule with")]
		public void ThenIShouldSeeScheduleInTeamScheduleWith(string name, Table table)
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(name);
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
			var layer = layers.FirstOrDefault();
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[0][1]);
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[1][1]);
		}


		[Then(@"I should see my colleague's absence")]
		public void ThenIShouldMyColleagueSAbsence()
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see the next day")]
		[Then(@"I should see tomorrow")]
		public void ThenIShouldSeeTheNextDay()
		{
			AssertShowingDay(DateOnlyForBehaviorTests.TestToday.AddDays(1));
		}

		[Then(@"I should see the previous day")]
		public void ThenIShouldSeeThePreviousDay()
		{
			AssertShowingDay(DateOnlyForBehaviorTests.TestToday.AddDays(-1));
		}

		private void AssertShowingDay(DateOnly date)
		{
			Browser.Interactions.AssertUrlContains(string.Format("{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
		}

		[Then(@"I should not see the absence's color")]
		public void ThenIShouldNotSeeTheAbsenceSColor()
		{
			var collLayers = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			var color = TestData.ConfidentialAbsence.DisplayColor;
			var colorAsString = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);

			foreach (var collLayer in collLayers)
			{
				var styleText = collLayer.Style.CssText;
				styleText.Should().Not.Contain(colorAsString);
			}
		}

		[Then(@"I should see my colleague's day off")]
		public void ThenIShouldSeeMyColleagueSDayOff()
		{
			var dayOff = Pages.Pages.TeamSchedulePage.DayOffByAgentName(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			EventualAssert.That(() => dayOff.Exists, Is.True);
		}

		[Then(@"The time line should span from (.*) to (.*)")]
		public void ThenTheTimeLineShouldSpanFromAToB(string from, string to)
		{
			EventualAssert.That(() => Pages.Pages.TeamSchedulePage.FirstTimeLineItem().InnerHtml, Contains.Substring(from));
			EventualAssert.That(() => Pages.Pages.TeamSchedulePage.LastTimeLineItem().InnerHtml, Contains.Substring(to));
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
			var colleague = Pages.Pages.TeamSchedulePage.AgentByName(DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString());
			var mySelf = Pages.Pages.TeamSchedulePage.AgentByName(DataMaker.Data().MePerson.Name.ToString());
			
			EventualAssert.That(() => colleague.Exists, Is.True);
			EventualAssert.That(() => mySelf.Exists, Is.True);

			var all = new List<Div>(Pages.Pages.TeamSchedulePage.Agents());
			all.IndexOf(colleague)
				.Should().Be.LessThan(all.IndexOf(mySelf));
		}

		[Then(@"I should see '(.*)' before myself")]
		public void ThenIShouldSeeBeforeMyself(string name)
		{
			var colleague = Pages.Pages.TeamSchedulePage.AgentByName(name);
			var mySelf = Pages.Pages.TeamSchedulePage.AgentByName(DataMaker.Data().MePerson.Name.ToString());

			EventualAssert.That(() => colleague.Exists, Is.True);
			EventualAssert.That(() => mySelf.Exists, Is.True);

			var all = new List<Div>(Pages.Pages.TeamSchedulePage.Agents());
			all.IndexOf(colleague)
				.Should().Be.LessThan(all.IndexOf(mySelf));
		}


		[Then(@"I should see myself without schedule")]
		public void ThenIShouldSeeMyselfWithoutSchedule()
		{
			var name = DataMaker.Data().MePerson.Name.ToString();
			AssertAgentIsDisplayed(name);
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(name);
			EventualAssert.That(() => layers.Count, Is.EqualTo(0));
		}

		[Then(@"I should see my colleague without schedule")]
		public void ThenIShouldSeeMyColleagueWithoutSchedule()
		{
			var name = DataMaker.Person(ColleagueStepDefinitions.TeamColleagueName).Person.Name.ToString();
			AssertAgentIsDisplayed(name);
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(name);
			EventualAssert.That(() => layers.Count, Is.EqualTo(0));
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
		public void ThenIShouldSeeAvailableGroupOptions(string teamGroup,Table table)
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
			var teams = new List<string> {lastTeam, firstTeam}.OrderBy(t => t).ToArray();

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
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Data().MePerson.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-text").Should().Contain(startTime);
		}

		[Then(@"The layer's end time attibute value should be (.*)")]
		public void ThenTheLayerSEndTimeAttibuteValueShouldBe(string endTime)
		{
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Data().MePerson.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-text").Should().Contain(endTime);
		}

		[Then(@"The layer's activity name attibute value should be (.*)")]
		public void ThenTheLayerSActivityNameAttibuteValueShouldBe(String activityName)
		{
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(DataMaker.Data().MePerson.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-title").Should().Contain(activityName);
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
			Browser.Interactions.AssertFirstContains(".select2-container .select2-choice span", theOtherSitesTeam);
		}

		[Then(@"the team-picker should have my team selected")]
		public void ThenTheTeam_PickerShouldHaveMyTeamSelected()
		{
			var myTeam = DataMaker.Data().UserData<Team>().TheTeam.Id.Value.ToString();
			Select2Box.AssertSelectedOptionValue("Team-Picker", myTeam);
		}

		[Then(@"the team-picker should have the first of the other site's teams selected")]
		public void ThenTheTeam_PickerShouldHaveTheFirstOfTheOtherSiteSTeamsSelected()
		{
			var team1 = DataMaker.Data().UserData<AnotherSitesTeam>().TheTeam;
			var team2 = DataMaker.Data().UserData<AnotherSitesSecondTeam>().TheTeam;
			var expected = new[] {team1, team2}.OrderBy(t => t.Description.Name).First();

			Select2Box.AssertSelectedOptionValue("Team-Picker", expected.Id.Value.ToString());
		}

		[Then(@"I should not see the team-picker")]
		public void ThenIShouldNotSeeTheTeam_Picker()
		{
			Browser.Interactions.AssertNotExists(".navbar-nav", "#Team-Picker");
		}

		[Then(@"I should see the team-picker")]
		public void ThenIShouldSeeTheTeam_PickerWithTwoTeams()
		{
			Browser.Interactions.AssertExists("#Team-Picker");
		}

		[Then(@"I should not see shiftrade button")]
		public void ThenIShouldNotSeeShiftradeButton()
		{
			Browser.Interactions.AssertNotExists("#TeamSchedule-body",".glyphicon-random");
		}

		[When(@"I click any shifttrade button")]
		public void WhenIClickAnyShifttradeButton()
		{
			Browser.Interactions.Click(".glyphicon-random");
		}

        [When(@"I initialize a shift trade")]
        public void WhenIInitializeAShiftTrade()
        {
            Browser.Interactions.Click(".initialize-shift-trade");
        }
        
        [Then(@"I should not be able to initialize a shift trade")]
        public void ThenIShouldNotBeAbleToInitializeAShiftTrade()
        {
			//Browser.Interactions.AssertExists(".btn[disabled]>.initialize-shift-trade");
            Browser.Interactions.AssertNotExists(".navbar", ".initialize-shift-trade");
        }

		private static void AssertAgentIsDisplayed(string name)
		{
			Browser.Interactions.AssertAnyContains(".teamschedule-agent-name", name);
		}

		private static void AssertAgentIsNotDisplayed(string name)
		{
			var agent = Pages.Pages.TeamSchedulePage.AgentByName(name, false);
			EventualAssert.That(() => agent.Exists, Is.False);
		}

	}

}
