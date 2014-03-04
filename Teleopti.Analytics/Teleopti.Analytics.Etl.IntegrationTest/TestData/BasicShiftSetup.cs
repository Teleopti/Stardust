using System;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
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

		public static void AddThreeShifts(string onPerson,
									IShiftCategory shiftCategory,
									IActivity activityLunch,
									IActivity activityPhone)
		{
			AddShift(onPerson, DateTime.Today.AddDays(-1), 9, 8, shiftCategory, activityLunch, activityPhone);
			AddShift(onPerson, DateTime.Today.AddDays(0), 9, 8, shiftCategory, activityLunch, activityPhone);
			AddShift(onPerson, DateTime.Today.AddDays(1), 9, 8, shiftCategory, activityLunch, activityPhone);
		}

		public static void AddShift(string onPerson, 
									DateTime dayLocal, 
									int startHour, 
									int lenghtHour,
									IShiftCategory shiftCategory,
									IActivity activityLunch,
									IActivity activityPhone)
		{
			var shift = new ShiftForDate(dayLocal, TimeSpan.FromHours(startHour), TimeSpan.FromHours(startHour+lenghtHour), Scenario.Scenario, shiftCategory, activityPhone, activityLunch);

			Data.Person(onPerson).Apply(shift);

			var readModel = new ScheduleDayReadModel
			{
				PersonId = Data.Person(onPerson).Person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = dayLocal,
				StartDateTime = dayLocal.AddHours(startHour),
				EndDateTime = dayLocal.AddHours(startHour + lenghtHour),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};
			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
		}

		public static SiteConfigurable Site { get; set; }
		public static TeamConfigurable Team { get; set; }
		public static ContractConfigurable Contract { get; set; }
		public static ContractScheduleConfigurable ContractSchedule { get; set; }
		public static PartTimePercentageConfigurable PartTimePercentage { get; set; }
		public static ScenarioConfigurable Scenario { get; set; }
	}
}
