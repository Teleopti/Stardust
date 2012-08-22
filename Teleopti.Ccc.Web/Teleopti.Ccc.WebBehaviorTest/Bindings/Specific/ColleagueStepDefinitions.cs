using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class ColleagueStepDefinitions
	{

		[Given(@"I have a colleague")]
		[Given(@"I have another colleague")]
		public void GivenIHaveAColleague()
		{
			UserFactory.User().AddTeamColleague();
			UserFactory.User().LastColleague().Setup(new Agent());
			UserFactory.User().LastColleague().Setup(new SchedulePeriod());
			if (UserFactory.User().HasSetup<Team>())
			{
				var team = UserFactory.User().UserData<Team>().TheTeam;
				UserFactory.User().LastColleague().Setup(new PersonPeriod(team));
			}
			else
			{
				UserFactory.User().LastColleague().Setup(new PersonPeriod());
			}
			UserFactory.User().LastColleague().Setup(new ScheduleIsPublished());
		}

		[Given(@"I have a colleague in another team")]
		public void GivenIHaveAColleagueInAnotherTeam()
		{
			if (!UserFactory.User().HasSetup<AnotherTeam>())
				UserFactory.User().Setup(new AnotherTeam());
			var team = UserFactory.User().UserData<AnotherTeam>().TheTeam;
			UserFactory.User().AddColleague();
			UserFactory.User().LastColleague().Setup(new Agent());
			UserFactory.User().LastColleague().Setup(new SchedulePeriod());
			UserFactory.User().LastColleague().Setup(new PersonPeriod(team));
			UserFactory.User().LastColleague().Setup(new ScheduleIsPublished());
		}

		[Given(@"My colleague has a shift today")]
		public void GivenMyColleagueHasAShiftToday()
		{
			UserFactory.User().TeamColleague().Setup(new ShiftToday());
		}
		[Given(@"The colleague in the other team has a shift today")]
		public void GivenTheColleagueInTheOtherTeamHasAShiftToday()
		{
			UserFactory.User().LastColleague().Setup(new ShiftToday());
		}

		[Given(@"My colleague has an absence today")]
		[Given(@"My colleague has a full-day absence today")]
		public void GivenMyColleagueHasAnAbsenceToday()
		{
			UserFactory.User().TeamColleague().Setup(new AbsenceToday());
		}

		[Given(@"My colleague has a dayoff today")]
		public void GivenMyColleagueHasADayoffToday()
		{
			UserFactory.User().TeamColleague().Setup(new DayOffToday());
		}

		[Given(@"My colleague has a shift from (.*) to (.*)")]
		public void GivenMyColleagueHaveAShiftFrom900To1801(string from, string to)
		{
			UserFactory.User().TeamColleague().Setup(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to)));
		}

		[Given(@"My colleague has a confidential absence")]
		public void GivenMyColleagueHasAnConfidentialAbsence()
		{
			UserFactory.User().TeamColleague().Setup(new ShiftWithConfidentialAbsence());
		}

	}
}