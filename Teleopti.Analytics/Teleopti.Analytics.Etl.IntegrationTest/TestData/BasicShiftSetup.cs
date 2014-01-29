﻿using System;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest.TestData
{
	public static class BasicShiftSetup
	{
		public static void SetupBasicForShifts()
		{
			Site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			Team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			Contract = new ContractConfigurable { Name = "Kontrakt" };
			ContractSchedule = new ContractScheduleConfigurable { Name = "Kontraktsschema" };
			PartTimePercentage = new PartTimePercentageConfigurable { Name = "ppp" };

			Scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};
			
			Data.Apply(Site);
			Data.Apply(Team);
			Data.Apply(Contract);
			Data.Apply(ContractSchedule);
			Data.Apply(PartTimePercentage);
			Data.Apply(Scenario);
		}

		public static void AddPerson(out IPerson person, string name, string externalLogon)
		{
			person = TestState.TestDataFactory.Person(name).Person;
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = Contract.Contract.Description.Name,
				ContractSchedule = ContractSchedule.ContractSchedule.Description.Name,
				PartTimePercentage = PartTimePercentage.Name,
				Team = Team.Name,
				StartDate = DateTime.Today.AddDays(-6),
                ExternalLogon = externalLogon
			};
			Data.Person(name).Apply(pp);
			Data.Person(name).Apply(new StockholmTimeZone());
		}

		public static void AddThreeShifts(string onPerson)
		{
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat",Color = "Green"};
			var act = new ActivityConfigurable { Name = "Phone",Color = "LightGreen", InReadyTime = true};
			var act2 = new ActivityConfigurable { Name = "Lunch", Color = "Red"};
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), 9, Scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 9, Scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			var shift3 = new ShiftForDate(DateTime.Today.AddDays(1), 9, Scenario.Scenario, cat.ShiftCategory, act.Activity,
										  act2.Activity);

			Data.Person(onPerson).Apply(shift);
			Data.Person(onPerson).Apply(shift2);
			Data.Person(onPerson).Apply(shift3);
		}

		public static void AddOverlapping(string onPerson)
		{
			var cat = new ShiftCategoryConfigurable { Name = "Kattegat" };
			var act = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var act2 = new ActivityConfigurable { Name = "Lunch", Color = "Red" };
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), TimeSpan.FromHours(21),TimeSpan.FromHours(32), Scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 6, Scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);
			
			Data.Person(onPerson).Apply(shift);
			Data.Person(onPerson).Apply(shift2);
			
		}

		public static SiteConfigurable Site { get; set; }
		public static TeamConfigurable Team { get; set; }
		public static ContractConfigurable Contract { get; set; }
		public static ContractScheduleConfigurable ContractSchedule { get; set; }
		public static PartTimePercentageConfigurable PartTimePercentage { get; set; }
		public static ScenarioConfigurable Scenario { get; set; }
		
	}
}
