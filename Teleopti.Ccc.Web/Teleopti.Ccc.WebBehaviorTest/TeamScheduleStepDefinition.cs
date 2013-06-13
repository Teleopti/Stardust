using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;
using Div = WatiN.Core.Div;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TeamScheduleStepDefinition
	{
		[When(@"I select the other team in the team picker")]
		public void WhenIChooseTheOtherTeamInTheTeamPicker()
		{
			var team = UserFactory.User().UserData<AnotherTeam>().TheTeam;
			var site = GlobalDataContext.Data().Data<CommonSite>().Site.Description.Name;
			var id = team.Id.ToString();
			var text = site + "/" + team.Description.Name;
			Select2Box.SelectItemByIdAndText("Team-Picker", id, text);
		}

		[When(@"I select '(.*)' in the team picker")]
		public void WhenISelectInTheTeamPicker(string optionText)
		{
			IOpenTheTeamPicker();

			Select2Box.SelectItemByText("Team-Picker", optionText);
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
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see my schedule in team schedule with")]
		public void WhenIShouldSeeMyScheduleInTeamScheduleWith(Table table)
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
			var layer = layers.FirstOrDefault();
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[0][1]);
			layer.GetAttributeValue("tooltip-text").Should().Contain(table.Rows[1][1]);
		}

		[Then(@"I should see my colleague's schedule")]
		public void ThenIShouldSeeMyColleagueSSchedule()
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
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
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see the next day")]
		[Then(@"I should see tomorrow")]
		public void ThenIShouldSeeTheNextDay()
		{
			AssertShowingDay(DateOnly.Today.AddDays(1));
		}

		[Then(@"I should see date '(.*)'")]
		public void ThenIShouldSeeDate(DateTime date)
		{
			AssertShowingDay(new DateOnly(date));
		}


		[Then(@"I should see the previous day")]
		public void ThenIShouldSeeThePreviousDay()
		{
			AssertShowingDay(DateOnly.Today.AddDays(-1));
		}

		private void AssertShowingDay(DateOnly date)
		{
			Browser.Interactions.AssertUrlContains(string.Format("{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
		}

		[Then(@"I should not see the absence's color")]
		public void ThenIShouldNotSeeTheAbsenceSColor()
		{
			var collLayers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
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
			var dayOff = Pages.Pages.TeamSchedulePage.DayOffByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
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
			AssertAgentIsNotDisplayed(UserFactory.User().TeamColleague().Person.Name.ToString());
		}

		[Then(@"I should not see '(.*)' schedule")]
		public void ThenIShouldNotSeeSchedule(string name)
		{
			AssertAgentIsNotDisplayed(name);
		}

		[Then(@"I should not see the other colleague's schedule")]
		public void ThenIShouldNotSeeTheOtherColleagueSSchedule()
		{
			AssertAgentIsNotDisplayed(UserFactory.User().LastColleague().Person.Name.ToString());
		}

		[Then(@"I should see my colleague before myself")]
		public void ThenIShouldSeeMyColleagueBeforeMyself()
		{
			var colleague = Pages.Pages.TeamSchedulePage.AgentByName(UserFactory.User().LastColleague().Person.Name.ToString());
			var mySelf = Pages.Pages.TeamSchedulePage.AgentByName(UserFactory.User().Person.Name.ToString());
			
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
			var mySelf = Pages.Pages.TeamSchedulePage.AgentByName(UserFactory.User().Person.Name.ToString());

			EventualAssert.That(() => colleague.Exists, Is.True);
			EventualAssert.That(() => mySelf.Exists, Is.True);

			var all = new List<Div>(Pages.Pages.TeamSchedulePage.Agents());
			all.IndexOf(colleague)
				.Should().Be.LessThan(all.IndexOf(mySelf));
		}


		[Then(@"I should see myself without schedule")]
		public void ThenIShouldSeeMyselfWithoutSchedule()
		{
			var name = UserFactory.User().Person.Name.ToString();
			AssertAgentIsDisplayed(name);
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(name);
			EventualAssert.That(() => layers.Count, Is.EqualTo(0));
		}

		[Then(@"I should see my colleague without schedule")]
		public void ThenIShouldSeeMyColleagueWithoutSchedule()
		{
			var name = UserFactory.User().LastColleague().Person.Name.ToString();
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
			var site = GlobalDataContext.Data().Data<CommonSite>().Site.Description.Name;
			var myTeam = site + "/" + UserFactory.User().UserData<Team>().TheTeam.Description.Name;
			var otherTeam = site + "/" + UserFactory.User().UserData<AnotherTeam>().TheTeam.Description.Name;

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
			AssertAgentIsDisplayed(UserFactory.User().LastColleague().Person.Name.ToString());
		}

		[Then(@"I should see colleague '(.*)'")]
		public void ThenIShouldSeeColleague(string personName)
		{
			AssertAgentIsDisplayed(personName);
		}


		[Then(@"I should not see myself")]
		public void ThenIShouldNotSeeMySchedule()
		{
			AssertAgentIsNotDisplayed(UserFactory.User().Person.Name.ToString());
		}

		[Then(@"The layer's start time attibute value should be (.*)")]
		public void ThenTheLayerSStartTimeAttibuteValueShouldBe(string startTime)
		{
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().Person.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-text").Should().Contain(startTime);
		}

		[Then(@"The layer's end time attibute value should be (.*)")]
		public void ThenTheLayerSEndTimeAttibuteValueShouldBe(string endTime)
		{
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().Person.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-text").Should().Contain(endTime);
		}

		[Then(@"The layer's activity name attibute value should be (.*)")]
		public void ThenTheLayerSActivityNameAttibuteValueShouldBe(String activityName)
		{
			var layer = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().Person.Name.ToString()).FirstOrDefault();

			layer.GetAttributeValue("tooltip-title").Should().Contain(activityName);
		}

		[Then(@"I should see the team-picker with the other site's team")]
		public void ThenIShouldSeeTheTeam_PickerWithTheOtherSiteSTeam()
		{
			var teamId = UserFactory.User().UserData<AnotherSitesTeam>().TheTeam.Id;

			var name = UserFactory.User().Person.Name.ToString();
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
			var theOtherSitesTeam = UserFactory.User().UserData<AnotherSitesTeam>().TheTeam.Description.Name;
			Browser.Interactions.AssertContains(".select2-container .select2-choice span", theOtherSitesTeam);
		}

		[Then(@"the team-picker should have my team selected")]
		public void ThenTheTeam_PickerShouldHaveMyTeamSelected()
		{
			var myTeam = UserFactory.User().UserData<Team>().TheTeam.Id.Value.ToString();
			Select2Box.AssertSelectedOptionValue("Team-Picker", myTeam);
		}

		[Then(@"the team-picker should have the first of the other site's teams selected")]
		public void ThenTheTeam_PickerShouldHaveTheFirstOfTheOtherSiteSTeamsSelected()
		{
			var team1 = UserFactory.User().UserData<AnotherSitesTeam>().TheTeam;
			var team2 = UserFactory.User().UserData<AnotherSitesSecondTeam>().TheTeam;
			var expected = new[] {team1, team2}.OrderBy(t => t.Description.Name).First();

			Select2Box.AssertSelectedOptionValue("Team-Picker", expected.Id.Value.ToString());
		}

		[Then(@"I should not see the team-picker")]
		public void ThenIShouldNotSeeTheTeam_Picker()
		{
			Browser.Interactions.AssertNotExists(".navbar-inner", "#Team-Picker");
		}

		[Then(@"I should see the team-picker")]
		public void ThenIShouldSeeTheTeam_PickerWithTwoTeams()
		{
			Browser.Interactions.AssertExists("#Team-Picker");
		}


		private static void AssertAgentIsDisplayed(string name)
		{
			TestControllerMethods.WaitUntilReadyForInteraction();
			Browser.Interactions.AssertExists(string.Format(".teamschedule-agent-name:contains('{0}')", name));
		}

		private static void AssertAgentIsNotDisplayed(string name)
		{
			var agent = Pages.Pages.TeamSchedulePage.AgentByName(name, false);
			EventualAssert.That(() => agent.Exists, Is.False, () => name + " found");
		}

	}
}
