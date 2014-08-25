using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
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
				var team = new Team();
				DataMaker.Data().Apply(team);
				DataMaker.Person(TeamColleagueName).Apply(new PersonPeriod(team.TheTeam));
			}
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published2", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Person(TeamColleagueName).Apply(new WorkflowControlSetForUser { Name = "Published2" });
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
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable { Name = "Published3", SchedulePublishedToDate = "2030-12-01" });
			DataMaker.Person(OtherTeamColleagueName).Apply(new WorkflowControlSetForUser { Name = "Published3" });
		}

		[Given(@"My colleague has an assigned shift with")]
		public void GivenMyColleagueHasAShiftWith(Table table)
		{
			DataMaker.ApplyFromTable<AssignedShift>(TeamColleagueName, table);
		}

		[Given(@"The colleague in the other team has an assigned shift with")]
		public void GivenTheColleagueInTheOtherTeamHasAShiftWith(Table table)
		{
			DataMaker.ApplyFromTable<AssignedShift>(OtherTeamColleagueName, table);
		}

		[Given(@"My colleague has an assigned absence with")]
		[Given(@"My colleague has an assigned full-day absence with")]
		public void GivenMyColleagueHasAnAbsenceWith(Table table)
		{
			DataMaker.ApplyFromTable<AssignedAbsence>(TeamColleagueName, table);
		}

		[Given(@"My colleague has an assigned dayoff with")]
		public void GivenMyColleagueHasADayoffToday(Table table)
		{
			DataMaker.ApplyFromTable<AssignedDayOff>(TeamColleagueName, table);
		}

		[Given(@"My colleague has a confidential absence with")]
		public void GivenMyColleagueHasAnConfidentialAbsence(Table table)
		{
			DataMaker.ApplyFromTable<ShiftWithConfidentialAbsence>(TeamColleagueName, table);
		}
	}
}