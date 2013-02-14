using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TeamStepDefinitions
	{
		[Given(@"there is a team with")]
		public void GivenThereIsATeamWith(Table table)
		{
			var team = table.CreateInstance<TeamConfigurable>();
			UserFactory.User().Setup(team);
		}

		[Given(@"there is a team member with")]
		public void GivenThereIsATeamMemberWith(Table table)
		{
			var personDetails = table.CreateInstance<BasicPersonConfigurable>();

			UserFactory.User().AddColleague();
			UserFactory.User().LastColleague().Setup(personDetails);
		}

		[Given(@"there is a person period for '(.*)' with")]
		public void GivenThereIsAPersonPeriodForWith(string personName, Table table)
		{
			var personPeriod = table.CreateInstance<PersonPeriodConfigurable>();

			var user = UserFactory.User().LastColleague();
			user.Setup(personPeriod);
		}

		[Given(@"I am a team leader for '(.*)' with role '(.*)'")]
		public void GivenIAmATeamLeaderFor(string teamName, string roleName)
		{
			UserFactory.User().Setup(new RoleForUser{Name = roleName, Team = teamName});

			UserFactory.User().MakeUser();
		}

		[Given(@"there is a shift with")]
		public void GivenThereIsAShiftWith(Table table)
		{
			var shift = table.CreateInstance<ReadModelShiftConfigurable>();

			var datafactory = new DataFactory(GlobalUnitOfWorkState.UnitOfWorkAction);
			datafactory.Setup(shift);
			datafactory.Apply();
		}

		[When(@"I view schedules for '(.*)'")]
		public void WhenIViewSchedules(string date)
		{
			TestControllerMethods.LogonForExistingUser(UserFactory.User().Person.ApplicationAuthenticationInfo.ApplicationLogOnName);
			Navigation.GotoAnywhereSchedule(date);
		}

		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).Text.Contains(personName), Is.True);
            EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).TableCell(QuicklyFind.ByClass("shift")).ChildrenOfType<WatiN.Core.List>().First().OwnListItems.Count, Is.GreaterThan(0));
		}

		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
            EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).Text.Contains(personName), Is.True);
            EventualAssert.That(() => Pages.Pages.AnywherePage.ScheduleTable.TableRow(QuicklyFind.ByClass("agent")).TableCell(QuicklyFind.ByClass("shift")).ChildrenOfType<WatiN.Core.List>().First().OwnListItems.Count, Is.EqualTo(0));
		}
	}
}