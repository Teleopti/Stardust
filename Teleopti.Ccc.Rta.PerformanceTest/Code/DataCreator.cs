using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class DataCreator
	{
		private readonly MutableNow _now;
		private readonly TestConfiguration _testConfiguration;
		private readonly Http _http;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public DataCreator(
			MutableNow now,
			TestConfiguration testConfiguration,
			Http http,
			ICurrentUnitOfWork unitOfWork
			)
		{
			_now = now;
			_testConfiguration = testConfiguration;
			_http = http;
			_unitOfWork = unitOfWork;
		}

		[LogTime]
		[UnitOfWork]
		public virtual void Create()
		{
			_now.Is("2016-02-25 08:00".Utc());

			_http.Get("/Test/SetCurrentTime?ticks=" + _now.UtcDateTime().Ticks);

			var data = new TestDataFactory(action =>
			{
				action.Invoke(_unitOfWork);
				_unitOfWork.Current().PersistAll();
			}, TenantUnitOfWorkState.TenantUnitOfWorkAction);

			var datasource = new Datasources(_testConfiguration.DataSourceId, " ", -1, " ", -1, " ", " ", 1, false, _testConfiguration.SourceId, false);
			new AnalyticsDataFactory().Apply(datasource);

			var businessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState;

			data.Apply(new ScenarioConfigurable { Name = "Default", BusinessUnit = businessUnit.Name });
			data.Apply(new ActivityConfigurable { Name = "Phone" });
			data.Apply(new ActivityConfigurable { Name = "Lunch" });
			data.Apply(new ActivityConfigurable { Name = "Break" });

			data.Apply(new ContractConfigurable { Name = "contract" });
			data.Apply(new PartTimePercentageConfigurable { Name = "partTimePercentage" });
			data.Apply(new ContractScheduleConfigurable { Name = "contractSchedule" });

			data.Apply(new RtaMapConfigurable { Activity = "Phone", PhoneState = "Ready", Adherence = "In", Name = "InAdherence" });
			data.Apply(new RtaMapConfigurable { Activity = "Phone", PhoneState = "LoggedOff", Adherence = "Out", Name = "OutOfAdherence" });

			data.Apply(new RtaMapConfigurable { Activity = "Break", PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence" });
			data.Apply(new RtaMapConfigurable { Activity = "Break", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence" });

			data.Apply(new RtaMapConfigurable { Activity = "Lunch", PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence" });
			data.Apply(new RtaMapConfigurable { Activity = "Lunch", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence" });

			data.Apply(new RtaMapConfigurable { Activity = null, PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence" });

			Enumerable.Range(0, (_testConfiguration.NumberOfAgents / 100) + 1)
				.ForEach(site =>
				{
					data.Apply(new SiteConfigurable
					{
						Name = "site" + site,
						BusinessUnit = businessUnit.Name
					});
				});

			Enumerable.Range(0, (_testConfiguration.NumberOfAgents / 10) + 1)
				.ForEach(team =>
				{
					data.Apply(new TeamConfigurable
					{
						Name = "team" + team,
						Site = "site" + (team / 10)
					});
				});

			Enumerable.Range(0, _testConfiguration.NumberOfAgents)
				.ForEach(roger =>
				{
					var name = "roger" + roger;

					Console.WriteLine("creating " + name);

					data.Person(name).Apply(new PersonPeriodConfigurable
					{
						StartDate = "2000-01-01 00:00".Utc(),
						ExternalLogon = name,
						ExternalLogonDataSourceId = 9,
						Contract = "contract",
						PartTimePercentage = "partTimePercentage",
						ContractSchedule = "contractSchedule",
						Team = "team" + (roger / 10)
					});

					data.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-25 08:00".Utc(), "2016-02-25 17:00".Utc())
						.AddActivity("Break", "2016-02-25 10:00".Utc(), "2016-02-25 10:15".Utc())
						.AddActivity("Lunch", "2016-02-25 11:30".Utc(), "2016-02-25 12:00".Utc())
						.AddActivity("Break", "2016-02-25 15:00".Utc(), "2016-02-25 15:15".Utc())
						);

					data.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-26 08:00".Utc(), "2016-02-26 17:00".Utc())
						.AddActivity("Break", "2016-02-26 10:00".Utc(), "2016-02-26 10:15".Utc())
						.AddActivity("Lunch", "2016-02-26 11:30".Utc(), "2016-02-26 12:00".Utc())
						.AddActivity("Break", "2016-02-26 15:00".Utc(), "2016-02-26 15:15".Utc())
						);

					data.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-27 08:00".Utc(), "2016-02-27 17:00".Utc())
						.AddActivity("Break", "2016-02-27 10:00".Utc(), "2016-02-27 10:15".Utc())
						.AddActivity("Lunch", "2016-02-27 11:30".Utc(), "2016-02-27 12:00".Utc())
						.AddActivity("Break", "2016-02-27 15:00".Utc(), "2016-02-27 15:15".Utc())
						);
				});
		}
	}
}