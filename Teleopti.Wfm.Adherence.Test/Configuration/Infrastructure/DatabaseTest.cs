using Autofac;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
	public abstract class DatabaseTest : INotCompatibleWithIoCTest
	{
		private ISession _session;
		private IUnitOfWork _unitOfWork;
		private IContainer container;
		
		protected IBusinessUnit _businessUnit;
		protected ISession Session => _session;
		protected IUnitOfWork UnitOfWork => _unitOfWork;
		protected ICurrentUnitOfWork CurrentUnitOfWork => new ThisUnitOfWork(_unitOfWork);
		
		[SetUp]
		public void Setup()
		{
			var (person, businessUnit) = InfrastructureTestSetup.Setup();
			_businessUnit = businessUnit;
			
			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"})));
			builder.RegisterType<FakeLicenseRepository>().AsSelf().As<ILicenseRepository>().SingleInstance();
			builder.RegisterInstance(new FakeConfigReader().FakeInfraTestConfig()).AsSelf().As<IConfigReader>().SingleInstance();
			container = builder.Build();
			
			var dataSource = container.Resolve<IDataSourceForTenant>().Tenant(InfraTestConfigReader.TenantName());
			Login(person, businessUnit, dataSource);
			_unitOfWork = dataSource.Application.CreateAndOpenUnitOfWork();
			_session = _unitOfWork.FetchSession();
			SetupForRepositoryTest();
		}

		protected virtual void SetupForRepositoryTest()
		{
		}

		[TearDown]
		public void BaseTeardown()
		{
			_unitOfWork.Dispose();
			_unitOfWork = null;
			container.Dispose();
		}

		protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
		{
			Session.SaveOrUpdate(obj);
			Session.Flush();
			Session.Evict(obj);
		}

		private void Login(IPerson person, IBusinessUnit businessUnit, IDataSource dataSource)
		{
			var principalContext = SelectivePrincipalContext.Make();
			var principal = TeleoptiPrincipalFactory.Make().MakePrincipal(new PersonAndBusinessUnit(person, businessUnit), dataSource, null);
			principalContext.SetCurrentPrincipal(principal);
		}
		
		protected void Logout()
		{
			var principalContext = SelectivePrincipalContext.Make();
			principalContext.SetCurrentPrincipal(null);
		}
	}
}