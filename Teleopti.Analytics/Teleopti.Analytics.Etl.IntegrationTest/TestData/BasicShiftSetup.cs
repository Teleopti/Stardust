using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public static class BasicShiftSetup
	{
		public static void SetupBasicForShifts(out IPerson person)
		{
			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			var contract = new ContractConfigurable { Name = "Kontrakt" };
			var cc = new ContractScheduleConfigurable { Name = "Kontraktsschema" };
			var ppp = new PartTimePercentageConfigurable { Name = "ppp" };
			var scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat" };
			var act = new ActivityConfigurable { Name = "Phone" };
			var act2 = new ActivityConfigurable { Name = "Lunch" };
			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(cc);
			Data.Apply(ppp);
			Data.Apply(scenario);
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 9, scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift3 = new ShiftForDate(DateTime.Today.AddDays(1), 9, scenario.Scenario, cat.ShiftCategory, act.Activity,
										  act2.Activity);
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = contract.Contract.Description.Name,
				ContractSchedule = cc.ContractSchedule.Description.Name,
				PartTimePercentage = ppp.Name,
				Team = team.Name,
				StartDate = DateTime.Today.AddDays(-6)
			};

			person = TestState.TestDataFactory.Person("Ola H").Person;
			Data.Person("Ola H").Apply(shift);
			Data.Person("Ola H").Apply(shift2);
			Data.Person("Ola H").Apply(shift3);
			Data.Person("Ola H").Apply(pp);
			Data.Person("Ola H").Apply(new StockholmTimeZone());
		}
	}
}
