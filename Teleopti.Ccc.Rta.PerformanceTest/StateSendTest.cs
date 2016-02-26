using System;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.PerformanceTest
{
	[TestFixture]
	[Category("SendStateTest")]
	public class StateSendTest
	{
		[Test]
		public void MeasurePerformance()
		{
			var factory = new TestDataFactory(GlobalUnitOfWorkState.UnitOfWorkAction, TenantUnitOfWorkState.TenantUnitOfWorkAction);

			factory.Apply(new ContractConfigurable {Name = "contract" });
			factory.Apply(new PartTimePercentageConfigurable {Name = "partTimePercentage"});
			factory.Apply(new ContractScheduleConfigurable{ Name = "contractSchedule" });
			factory.Apply(new SiteConfigurable {Name = "site", BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name});
			factory.Apply(new TeamConfigurable {Name = "team", Site = "site"});
			
			
			factory
				.Person("roger")
				.Apply(new PersonPeriodConfigurable
				{
					StartDate = DateTime.Now.Date,
					ExternalLogon = "roger",
					ExternalLogonDataSourceId = 6,

					Contract = "contract",
					PartTimePercentage = "partTimePercentage",
					ContractSchedule = "contractSchedule",
					Team = "team"
				});

			Thread.Sleep(TimeSpan.FromMinutes(new Random().Next(1, 2)));
		}
	}

	public class PersonSetup : IDataSetup
	{
		public string Name { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var personRepository = new PersonRepository(currentUnitOfWork);
			var person = new Person {Name = new Name(Name, "")};
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.UtcTimeZoneInfo());

			var personPeriod = new PersonPeriodConfigurable
			{
				ExternalLogon = Name
			};
			personPeriod.Apply(currentUnitOfWork.Current(), person, CultureInfoFactory.CreateEnglishCulture());

			personRepository.Add(person);	
		}

	}
}