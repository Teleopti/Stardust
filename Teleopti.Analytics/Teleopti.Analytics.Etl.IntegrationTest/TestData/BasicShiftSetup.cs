using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

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

			var jobParameters = new JobParameters(
				new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), 1, "UTC", 15, "",
				"False",
				CultureInfo.CurrentCulture,
				new FakeContainerHolder(), false
			)
			{
				Helper = new JobHelperForTest(new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null), null)
			};
			var result = new List<IJobResult>();
			JobStepBase step = new StageScenarioJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimScenarioJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new StageOvertimeJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
			step = new DimOvertimeJobStep(jobParameters);
			step.Run(new List<IJobStep>(), TestState.BusinessUnit, result, true);
		}

		public static void AddPerson(out IPerson person, string name, string externalLogon, DateTime testDate, int idInAnalytics=1)
		{
			person = TestState.TestDataFactory.Person(name).Person;
			var pp = new PersonPeriodConfigurable
			{
				BudgetGroup = "",
				Contract = Contract.Contract.Description.Name,
				ContractSchedule = ContractSchedule.ContractSchedule.Description.Name,
				PartTimePercentage = PartTimePercentage.Name,
				Team = Team.Name,
				StartDate = testDate.AddDays(-6),
                ExternalLogon = externalLogon
			};
			Data.Person(name).Apply(new StockholmTimeZone());
			Data.Person(name).Apply(pp);
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new Person(person, ExistingDatasources.DefaultRaptorDefaultDatasourceId,
				idInAnalytics, testDate.AddDays(-6), AnalyticsDate.Eternity.DateDate, 0, -2, 1,
				TestState.BusinessUnit.Id.Value, false, 1));
			analyticsDataFactory.Persist();
		}

		public static void AddThreeShifts(string onPerson,
									IShiftCategory shiftCategory,
									IActivity activityLunch,
									IActivity activityPhone,
									DateTime testDate)
		{
			AddShift(onPerson, testDate.AddDays(-1), 9, 8, shiftCategory, activityLunch, activityPhone);
			AddShift(onPerson, testDate.AddDays(0), 9, 8, shiftCategory, activityLunch, activityPhone);
			AddShift(onPerson, testDate.AddDays(1), 9, 8, shiftCategory, activityLunch, activityPhone);
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
