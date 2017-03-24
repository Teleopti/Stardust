using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.InfrastructureTest;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;

namespace Teleopti.Wfm.Test
{
	[UnitOfWorkTest]
	public class AbsenceRequestImmediateResponseTest
	{
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ICurrentBusinessUnit CurrentBusinessUnit;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IDataSourceScope DataSourceScope;
		public ImpersonateSystem ImpersonateSystem;
		public WithUnitOfWork WithUnitOfWork;



		[Test]
		public void Gront()
		{
			//RestoreCcc7Database();
			setup();

		}

		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		private void setup()
		{

			var uow = CurrentUnitOfWorkFactory.Current().CurrentUnitOfWork();

			TestState.UnitOfWork = uow;
			TestState.TestDataFactory = new TestDataFactory(new ThisUnitOfWork(uow), CurrentTenantSession, TenantUnitOfWork);
			//TestState.BusinessUnit = BusinessUnitRepository.LoadAll().FirstOrDefault();

			var site = new SiteConfigurable { BusinessUnit = TestState.BusinessUnit.Name, Name = "Västerhaninge" };
			var team = new TeamConfigurable { Name = "Yellow", Site = "Västerhaninge" };
			var contract = new ContractConfigurable { Name = "Kontrakt" };
			var contractSchedule = new ContractScheduleConfigurable { Name = "Kontraktsschema" };
			var partTimePercentage = new PartTimePercentageConfigurable { Name = "ppp" };

			var scenario = new ScenarioConfigurable
			{
				EnableReporting = true,
				Name = "Scenario",
				BusinessUnit = TestState.BusinessUnit.Name
			};

			Data.Apply(site);
			Data.Apply(team);
			Data.Apply(contract);
			Data.Apply(contractSchedule);
			Data.Apply(partTimePercentage);
			Data.Apply(scenario);

		}

	}
}