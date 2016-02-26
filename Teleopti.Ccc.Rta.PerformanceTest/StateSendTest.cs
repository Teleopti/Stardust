using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	public class StateSendTest
	{
		[Test]
		public void MeasurePerformance()
		{
			var numberOfSites = 2;
			var numberOfTeams = numberOfSites * 10;
			var numberOfAgents = numberOfTeams * 10;

			var factory = new TestDataFactory(GlobalUnitOfWorkState.UnitOfWorkAction, TenantUnitOfWorkState.TenantUnitOfWorkAction);

			factory.Apply(new ContractConfigurable {Name = "contract" });
			factory.Apply(new PartTimePercentageConfigurable {Name = "partTimePercentage"});
			factory.Apply(new ContractScheduleConfigurable{ Name = "contractSchedule" });

			Enumerable.Range(0, numberOfSites)
				.ForEach(site =>
				{
					factory.Apply(new SiteConfigurable
					{
						Name = "site" + site,
						BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name
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
					factory
						.Person("roger" + roger) // == sant!
						.Apply(new PersonPeriodConfigurable
						{
							StartDate = DateTime.Now.Date,
							ExternalLogon = "roger" + roger,
							ExternalLogonDataSourceId = 6,

							Contract = "contract",
							PartTimePercentage = "partTimePercentage",
							ContractSchedule = "contractSchedule",
							Team = "team" + (roger / 10)
						});
				});

		}
	}
}