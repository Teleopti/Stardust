using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	public class IoC2TestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{

			base.Setup(system, configuration);
		}
	}

	[TestFixture]
	[Category("SendStateTest")]
	//[IoC2Test]
	public class StateSendTest
	{
		// this is the analytics data source id
		// and is saved with the external logon in wfm
		public static int DataSourceId = 9;

		// this is the switch id
		// and is sent with the state
		public static string SourceId = "8";

		[Test]
		public void MeasurePerformance()
		{
			var watch1 = Stopwatch.StartNew();
			var time = new MutableNow();
			time.Is("2016-02-25 08:00".Utc());

			var businessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name;
			var numberOfAgents = 5000;

			Http.Get("/Test/SetCurrentTime?ticks=" + time.UtcDateTime().Ticks);

			var datasource = new Datasources(DataSourceId, " ", -1, " ", -1, " ", " ", 1, false, SourceId, false);
			new AnalyticsDataFactory().Apply(datasource);

			var factory = new TestDataFactory(GlobalUnitOfWorkState.UnitOfWorkAction, TenantUnitOfWorkState.TenantUnitOfWorkAction);

			factory.Apply(new ScenarioConfigurable { Name = "Default", BusinessUnit = businessUnit });
			factory.Apply(new ActivityConfigurable { Name = "Phone" });
			factory.Apply(new ActivityConfigurable { Name = "Lunch" });
			factory.Apply(new ActivityConfigurable { Name = "Break" });

			factory.Apply(new ContractConfigurable {Name = "contract" });
			factory.Apply(new PartTimePercentageConfigurable {Name = "partTimePercentage"});
			factory.Apply(new ContractScheduleConfigurable{ Name = "contractSchedule" });
			
			factory.Apply(new RtaMapConfigurable { Activity = "Phone", PhoneState = "Ready", Adherence = "In", Name = "InAdherence"});
			factory.Apply(new RtaMapConfigurable { Activity = "Phone", PhoneState = "LoggedOff", Adherence = "Out", Name = "OutOfAdherence" });

			factory.Apply(new RtaMapConfigurable { Activity = "Break", PhoneState = "LoggedOff", Adherence = "In",  Name = "InAdherence" });
			factory.Apply(new RtaMapConfigurable { Activity = "Break", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence" });

			factory.Apply(new RtaMapConfigurable { Activity = "Lunch", PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence" });
			factory.Apply(new RtaMapConfigurable { Activity = "Lunch", PhoneState = "Ready", Adherence = "Out", Name = "OutOfAdherence" });
			
			factory.Apply(new RtaMapConfigurable { PhoneState = "LoggedOff", Adherence = "In", Name = "InAdherence" });
			
			Enumerable.Range(0, numberOfAgents / 100)
				.ForEach(site =>
				{
					factory.Apply(new SiteConfigurable
					{
						Name = "site" + site,
						BusinessUnit = businessUnit
					});
				});

			Enumerable.Range(0, numberOfAgents / 10)
				.ForEach(team =>
				{
					factory.Apply(new TeamConfigurable
					{
						Name = "team" + team,
						Site = "site" + (team / 10)
					});
				});

			Enumerable.Range(0, numberOfAgents)
				.ForEach(roger =>
				{
					var name = "roger" + roger;
					factory.Person(name).Apply(new PersonPeriodConfigurable
					{
						StartDate = "2000-01-01 00:00".Utc(),
						ExternalLogon = name,
						ExternalLogonDataSourceId = 9,

						Contract = "contract",
						PartTimePercentage = "partTimePercentage",
						ContractSchedule = "contractSchedule",
						Team = "team" + (roger/10)
					});

					factory.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-25 08:00".Utc(), "2016-02-25 17:00".Utc())
						.AddActivity("Break", "2016-02-25 10:00".Utc(), "2016-02-25 10:15".Utc())
						.AddActivity("Lunch", "2016-02-25 11:30".Utc(), "2016-02-25 12:00".Utc())
						.AddActivity("Break", "2016-02-25 15:00".Utc(), "2016-02-25 15:15".Utc())
						);

					factory.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-26 08:00".Utc(), "2016-02-26 17:00".Utc())
						.AddActivity("Break", "2016-02-26 10:00".Utc(), "2016-02-26 10:15".Utc())
						.AddActivity("Lunch", "2016-02-26 11:30".Utc(), "2016-02-26 12:00".Utc())
						.AddActivity("Break", "2016-02-26 15:00".Utc(), "2016-02-26 15:15".Utc())
						);

					factory.Person(name).Apply(new ShiftConfigurable
					{
						Scenario = "Default"
					}
						.AddActivity("Phone", "2016-02-27 08:00".Utc(), "2016-02-27 17:00".Utc())
						.AddActivity("Break", "2016-02-27 10:00".Utc(), "2016-02-27 10:15".Utc())
						.AddActivity("Lunch", "2016-02-27 11:30".Utc(), "2016-02-27 12:00".Utc())
						.AddActivity("Break", "2016-02-27 15:00".Utc(), "2016-02-27 15:15".Utc())
						);
				});

			var changes = new[]
			{
				new TimeStateInfo {Time = "2016-02-26 08:01", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 09:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 09:05", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 10:00", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 10:15", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 11:35", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 11:55", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 14:55", StateCode = "LoggedOff"},
				new TimeStateInfo {Time = "2016-02-26 15:15", StateCode = "Ready"},
				new TimeStateInfo {Time = "2016-02-26 17:05", StateCode = "LoggedOff"},
			};
			Console.WriteLine(watch1.Elapsed);
			Console.WriteLine("******************************");


			var watch = Stopwatch.StartNew();
			changes.ForEach(i =>
			{
				time.Is(i.Time.Utc());
				Http.Get("/Test/SetCurrentTime?ticks=" + time.UtcDateTime().Ticks);
				Enumerable.Range(0, numberOfAgents)
					.ForEach(roger =>
					{
						Http.PostJson(
							"Rta/State/Change",
							new ExternalUserStateWebModel
							{
								AuthenticationKey = LegacyAuthenticationKey.TheKey,
								UserCode = "roger" + roger,
								StateCode = i.StateCode,
								IsLoggedOn = true,
								PlatformTypeId = Guid.Empty.ToString(),
								SourceId = SourceId,
								IsSnapshot = false
							});
					});
			});
			Console.WriteLine(watch.Elapsed);
		}

		public class TimeStateInfo
		{
			public string Time { get; set; }
			public string StateCode { get; set; }
		}
	}
}