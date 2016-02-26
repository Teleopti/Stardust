using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	public class StateSendTest
	{
		[Test]
		public void MeasurePerformance()
		{
			var businessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name;
			var numberOfSites = 2;
			var numberOfTeams = numberOfSites * 10;
			var numberOfAgents = numberOfTeams * 10;

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
			
			Enumerable.Range(0, numberOfSites)
				.ForEach(site =>
				{
					factory.Apply(new SiteConfigurable
					{
						Name = "site" + site,
						BusinessUnit = businessUnit
					});
				});

			Enumerable.Range(0, numberOfTeams)
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
					var personDataFactory = factory.Person("roger" + roger); // == sant!
					personDataFactory
						.Apply(new PersonPeriodConfigurable
						{
							StartDate = "2000-01-01 00:00".Utc(),
							ExternalLogon = "roger" + roger,
							ExternalLogonDataSourceId = 6,

							Contract = "contract",
							PartTimePercentage = "partTimePercentage",
							ContractSchedule = "contractSchedule",
							Team = "team" + (roger/10)
						});

					personDataFactory
						.Apply(new ShiftConfigurable
						{
							Scenario = "Default"
						}
							.AddActivity("Phone", "2016-02-25 08:00".Utc(), "2016-02-25 17:00".Utc())
							.AddActivity("Break", "2016-02-25 10:00".Utc(), "2016-02-25 10:15".Utc())
							.AddActivity("Lunch", "2016-02-25 11:30".Utc(), "2016-02-25 12:00".Utc())
							.AddActivity("Break", "2016-02-25 15:00".Utc(), "2016-02-25 15:15".Utc())
						);
					personDataFactory
						.Apply(new ShiftConfigurable
						{
							Scenario = "Default"
						}
							.AddActivity("Phone", "2016-02-26 08:00".Utc(), "2016-02-26 17:00".Utc())
							.AddActivity("Break", "2016-02-26 10:00".Utc(), "2016-02-26 10:15".Utc())
							.AddActivity("Lunch", "2016-02-26 11:30".Utc(), "2016-02-26 12:00".Utc())
							.AddActivity("Break", "2016-02-26 15:00".Utc(), "2016-02-26 15:15".Utc())
						);
					personDataFactory
						.Apply(new ShiftConfigurable
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