using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	[Binding]
	public class ColleagueStepDefinitions
	{
		public static string TeamColleagueName = "Colleague In Team";
		public static string OtherTeamColleagueName = "Colleague Not In Team";

		[Given(@"I have a colleague")]
		public void GivenIHaveAColleague()
		{
			DataMaker.Person(TeamColleagueName).Apply(new Agent());
			DataMaker.Person(TeamColleagueName).Apply(new SchedulePeriod());
			if (DataMaker.Data().HasSetup<Team>())
			{
				var team = DataMaker.Data().UserData<Team>().TheTeam;
				DataMaker.Person(TeamColleagueName).Apply(new PersonPeriod(team));
			}
			else
			{
				DataMaker.Person(TeamColleagueName).Apply(new PersonPeriod());
			}
			DataMaker.Person(TeamColleagueName).Apply(new ScheduleIsPublished());
		}

		[Given(@"I have a colleague in another team")]
		public void GivenIHaveAColleagueInAnotherTeam()
		{
			if (!DataMaker.Data().HasSetup<AnotherTeam>())
				DataMaker.Data().Apply(new AnotherTeam());
			var team = DataMaker.Data().UserData<AnotherTeam>().TheTeam;
			DataMaker.Person(OtherTeamColleagueName).Apply(new Agent());
			DataMaker.Person(OtherTeamColleagueName).Apply(new SchedulePeriod());
			DataMaker.Person(OtherTeamColleagueName).Apply(new PersonPeriod(team));
			DataMaker.Person(OtherTeamColleagueName).Apply(new ScheduleIsPublished());
		}

		[Given(@"My colleague has a shift today")]
		public void GivenMyColleagueHasAShiftToday()
		{
			DataMaker.Person(TeamColleagueName).Apply(new ShiftToday());
		}

		[Given(@"The colleague in the other team has a shift today")]
		public void GivenTheColleagueInTheOtherTeamHasAShiftToday()
		{
			DataMaker.Person(OtherTeamColleagueName).Apply(new ShiftToday());
		}

		[Given(@"My colleague has an absence today")]
		[Given(@"My colleague has a full-day absence today")]
		public void GivenMyColleagueHasAnAbsenceToday()
		{
			DataMaker.Person(TeamColleagueName).Apply(new AbsenceToday());
		}

		[Given(@"My colleague has a dayoff today")]
		public void GivenMyColleagueHasADayoffToday()
		{
			DataMaker.Person(TeamColleagueName).Apply(new DayOffToday());
		}

		[Given(@"My colleague has a shift from (.*) to (.*)")]
		public void GivenMyColleagueHaveAShiftFrom900To1801(string from, string to)
		{
			DataMaker.Person(TeamColleagueName).Apply(new ShiftToday(TimeSpan.Parse(from), TimeSpan.Parse(to)));
		}

		[Given(@"My colleague has a confidential absence")]
		public void GivenMyColleagueHasAnConfidentialAbsence()
		{
			DataMaker.Person(TeamColleagueName).Apply(new ShiftWithConfidentialAbsence());
		}

	}
}