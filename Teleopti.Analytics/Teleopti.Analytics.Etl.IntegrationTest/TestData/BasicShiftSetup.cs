using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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

		public static void SeparateOverlapping(string person, DateOnly today)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{

			//var _rep = RepositoryFactory.CreatePersonAssignmentRepository(uow);
			//var searchPeriod = new DateOnlyPeriod(today, today);
			//IList<IPerson> persons = new List<IPerson> { person };
			//ICollection<IPersonAssignment> retList = _rep.Find(persons, searchPeriod, Scenario.Scenario);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat" };
			var act = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var act2 = new ActivityConfigurable { Name = "Lunch", Color = "Red" };
			Data.Apply(cat);
			Data.Apply(act);
			Data.Apply(act2);

			var shift = new ShiftForDate(DateTime.Today.AddDays(-1), TimeSpan.FromHours(17), TimeSpan.FromHours(30), Scenario.Scenario, cat.ShiftCategory, act.Activity,
										 act2.Activity);
			var shift2 = new ShiftForDate(DateTime.Today, 7, Scenario.Scenario, cat.ShiftCategory, act.Activity, act2.Activity);

			Data.Person(person).Apply(shift);
			Data.Person(person).Apply(shift2);
			}

			// Handle readmodel
			var readModel = new ScheduleDayReadModel
			{
				PersonId = Data.Person(person).Person.Id.GetValueOrDefault(),
				ColorCode = 0,
				ContractTimeTicks = 500,
				Date = DateTime.Today,
				StartDateTime = DateTime.Today.AddDays(-15).AddHours(8),
				EndDateTime = DateTime.Today.AddDays(-15).AddHours(17),
				Label = "LABEL",
				NotScheduled = false,
				Workday = true,
				WorkTimeTicks = 600
			};

			var readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(-1);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
			Data.Apply(readM);
			readModel.Date = DateTime.Today.AddDays(0);
			readM = new ScheduleDayReadModelSetup { Model = readModel };
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
