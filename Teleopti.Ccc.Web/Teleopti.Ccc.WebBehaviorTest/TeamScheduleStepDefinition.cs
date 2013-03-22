﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Div = WatiN.Core.Div;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TeamScheduleStepDefinition
	{
		[When(@"I click on a day")]
		public void WhenIClickOnADay()
		{
			var date = new DateOnly(DateOnly.Today.Year, DateOnly.Today.Month, 1);
			Pages.Pages.CurrentDateRangeSelector.DatePicker.ClickDay(date);
		}

		[When(@"I select the other team in the team picker")]
		public void WhenIChooseTheOtherTeamInTheTeamPicker()
		{
			var team = UserFactory.User().UserData<AnotherTeam>().TheTeam;
			var site = GlobalDataContext.Data().Data<CommonSite>().Site.Description.Name;
			Browser.Current.Eval("$('#" + Pages.Pages.TeamSchedulePage.TeamPickerInput.Id + "').select2('data', {id:'" + team.Id + "', text:'" + site + "/" + team.Description.Name + "'}).trigger('change')");
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

		[Then(@"I should see my colleague's schedule")]
		public void ThenIShouldSeeMyColleagueSSchedule()
		{
			var layers = Pages.Pages.TeamSchedulePage.LayersByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
			EventualAssert.That(() => layers.Count, Is.GreaterThan(0));
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

		[Then(@"I should see the previous day")]
		public void ThenIShouldSeeThePreviousDay()
		{
			AssertShowingDay(DateOnly.Today.AddDays(-1));
		}

		[Then(@"I should see the selected day")]
		public void ThenIShouldSeeTheSelectedDay()
		{
			var date = new DateOnly(DateOnly.Today.Year, DateOnly.Today.Month, 1);
			AssertShowingDay(date);
		}

		private void AssertShowingDay(DateOnly date)
		{
			Browser.Current.Url.EndsWith(string.Format("{0}/{1}/{2}", date.Year, date.Month, date.Day));
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

		[Then(@"I should not see my colleague's day off")]
		public void ThenIShouldNotSeeMyColleagueSDayOff()
		{
			var dayOff = Pages.Pages.TeamSchedulePage.DayOffByAgentName(UserFactory.User().TeamColleague().Person.Name.ToString());
			EventualAssert.That(() => dayOff.Exists, Is.False);
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
			Browser.Current.Eval("$('#" + Pages.Pages.TeamSchedulePage.TeamPickerInput.Id + "').select2('open')");
		}

		[Then(@"I should see the team-picker with both teams")]
		public void ThenIShouldSeeTheTeam_PickerWithBothTeams()
		{
			var site = GlobalDataContext.Data().Data<CommonSite>().Site.Description.Name;
			var myTeam = site + "/" + UserFactory.User().UserData<Team>().TheTeam.Description.Name;
			var otherTeam = site + "/" + UserFactory.User().UserData<AnotherTeam>().TheTeam.Description.Name;

			AssertTeamPickerHasTeams(new[] {myTeam, otherTeam});
		}

		private static void AssertTeamPickerHasTeams(IEnumerable<string> teamNames)
		{
			var page = Pages.Pages.TeamSchedulePage;
			if (!page.TeamPickerDropDown.Exists)
				Browser.Current.Eval("$('#" + page.TeamPickerInput.Id + "').select2('open')");

			EventualAssert.That(() => page.TeamPickerSelectDiv.Exists, Is.True);
			EventualAssert.That(() => page.TeamPickerSelectDiv.JQueryVisible(), Is.True);
			EventualAssert.That(() => page.TeamPickerSelectDiv.DisplayVisible(), Is.True);
			var texts = Pages.Pages.TeamSchedulePage.TeamPickerSelectTexts();
			teamNames.ToList().ForEach(e => EventualAssert.That(() => texts.Contains(e), Is.True));
		}

		[Then(@"the teams should be sorted alphabetically")]
		public void ThenTheTeamsShouldBeSortedAlphabetically()
		{
			var actual = Pages.Pages.TeamSchedulePage.TeamPickerSelectTexts();
			var sorted = actual.OrderBy(t => t).ToArray();

			actual.Should().Have.SameSequenceAs(sorted);
		}

		[Then(@"I should see my colleague")]
		public void ThenIShouldSeeMyColleague()
		{
			AssertAgentIsDisplayed(UserFactory.User().LastColleague().Person.Name.ToString());
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
			var site = GlobalDataContext.Data().Data<AnotherSite>().Site.Description.Name;
			var theOtherSitesTeam = site + "/" + UserFactory.User().UserData<AnotherSitesTeam>().TheTeam.Description.Name;

			AssertTeamPickerHasTeams(new[] {theOtherSitesTeam});
		}

		[Then(@"I should see the other site's team")]
		public void ThenIShouldSeeTheOtherSiteSTeam()
		{
			var theOtherSitesTeam = UserFactory.User().UserData<AnotherSitesTeam>().TheTeam.Description.Name;
			EventualAssert.That(() => Pages.Pages.TeamSchedulePage.TeamPickerSelectLink().Text, Contains.Substring(theOtherSitesTeam));
		}

		[Then(@"the team-picker should have my team selected")]
		public void ThenTheTeam_PickerShouldHaveMyTeamSelected()
		{
			var myTeam = UserFactory.User().UserData<Team>().TheTeam.Id.Value.ToString();
			EventualAssert.That(() => Pages.Pages.TeamSchedulePage.TeamPickerInput.Value, Contains.Substring(myTeam));
		}

		[Then(@"the team-picker should have the first of the other site's teams selected")]
		public void ThenTheTeam_PickerShouldHaveTheFirstOfTheOtherSiteSTeamsSelected()
		{
			var team1 = UserFactory.User().UserData<AnotherSitesTeam>().TheTeam;
			var team2 = UserFactory.User().UserData<AnotherSitesSecondTeam>().TheTeam;
			var expected = new[] {team1, team2}.OrderBy(t => t.Description.Name).First();

			EventualAssert.That(() => Pages.Pages.TeamSchedulePage.TeamPickerInput.Value, Contains.Substring(expected.Id.Value.ToString()));
		}

		[Then(@"I should not see the team-picker")]
		public void ThenIShouldNotSeeTheTeam_Picker()
		{
			var picker = Pages.Pages.TeamSchedulePage.TeamPickerSelectDiv;
			Func<bool> visible = () => picker.Exists && picker.JQueryVisible();
			EventualAssert.That(visible, Is.False);
		}

		[Then(@"I should see the team-picker")]
		public void ThenIShouldSeeTheTeam_PickerWithTwoTeams()
		{
			var picker = Pages.Pages.TeamSchedulePage.TeamPickerSelectDiv;

			picker.Should().Not.Be.Null();
		}


		private static void AssertAgentIsDisplayed(string name)
		{
			var agent = Pages.Pages.TeamSchedulePage.AgentByName(name);
			EventualAssert.That(() => agent.Exists, Is.True, name + " not found");
		}

		private static void AssertAgentIsNotDisplayed(string name)
		{
			var agent = Pages.Pages.TeamSchedulePage.AgentByName(name);
			EventualAssert.That(() => agent.Exists, Is.False, name + " found");
		}

	}
}
