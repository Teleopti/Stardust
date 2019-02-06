using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[TestFixture]
	public class CreateBusinessUnitTest
	{
		private CreateBusinessUnit _target;
		private IDataSourcesFactory _dataSourcesFactory;
		private IRunWithUnitOfWork _runWithUnitOfWork;
		private IBusinessUnitRepository _businessUnitRepository;
		private IPersonRepository _personRepository;
		private IScenarioRepository _scenarioRepository;
		private IApplicationRoleRepository _applicationRoleRepository;
		private IAvailableDataRepository _availableDataRepository;
		private IKpiRepository _kpiRepository;
		private ISkillTypeRepository _skillTypeRepository;
		private IRtaStateGroupRepository _rtaStateGroupRepository;

		[SetUp]
		public void Setup()
		{
			_dataSourcesFactory = new FakeDataSourcesFactory(new FakeStorage(), null, null);
			_runWithUnitOfWork = new FakeRunWithUnitOfWork();
			_businessUnitRepository = new FakeBusinessUnitRepository();
			_personRepository = new FakePersonRepositoryLegacy();
			_scenarioRepository = new FakeScenarioRepository();
			_applicationRoleRepository =  new FakeApplicationRoleRepository();
			_availableDataRepository = new FakeAvailableDataRepository();
			_kpiRepository = new FakeKpiRepository();
			_skillTypeRepository = new FakeSkillTypeRepository();
			_rtaStateGroupRepository = new FakeRtaStateGroupRepositoryLegacy();

			_target = new CreateBusinessUnit(
				_dataSourcesFactory,
				_runWithUnitOfWork,
				uow => _businessUnitRepository,
				uow => _personRepository,
				uow => _scenarioRepository,
				uow => _applicationRoleRepository,
				uow => _availableDataRepository,
				uow => _kpiRepository,
				uow => _skillTypeRepository,
				uow => _rtaStateGroupRepository);
		}

		[Test]
		public void ShouldAddBusinessUnitsAndDefaultData()
		{
			var tenant = new Tenant("TestTenant");
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString("Data Source=.;Integrated Security=SSPI;Initial Catalog=Analytics;Application Name=Test");
			tenant.DataSourceConfiguration.SetApplicationConnectionString("Data Source=.;Integrated Security=SSPI;Initial Catalog=CCC7;Application Name=Test");
			const string businessUnitName = "TestBusinessUnit";

			_target.Create(tenant, businessUnitName);

			var businessUnits = _businessUnitRepository.LoadAll();
			businessUnits.Should().Not.Be.Empty();
			businessUnits.First().Name.Should().Be.EqualTo(businessUnitName);

			var scenarios = _scenarioRepository.LoadAll();
			scenarios.Should().Not.Be.Empty();
			var scenario = scenarios.First();
			scenario.Description.Name.Should().Be.EqualTo("Default");
			scenario.DefaultScenario.Should().Be.True();
			scenario.EnableReporting.Should().Be.True();

			var applicationRoles = _applicationRoleRepository.LoadAll();
			applicationRoles.Count().Should().Be.EqualTo(5);

			var availabledata = _availableDataRepository.LoadAll();
			availabledata.Count().Should().Be.EqualTo(5);

			var kpis = _kpiRepository.LoadAll();
			kpis.Count().Should().Be.EqualTo(7);

			var skillTypes = _skillTypeRepository.LoadAll();
			skillTypes.Count().Should().Be.EqualTo(6);

			var stateGroup = _rtaStateGroupRepository.LoadAll();
			stateGroup.Count().Should().Be.EqualTo(1);
		}
	}
}