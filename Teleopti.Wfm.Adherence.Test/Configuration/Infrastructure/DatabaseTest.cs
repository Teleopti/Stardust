using Autofac;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Configuration.Infrastructure
{
	public abstract class DatabaseTest : INotCompatibleWithIoCTest
	{
		protected IContainer Container;
		protected IBusinessUnit BusinessUnit;
		protected ISession Session => CurrentUnitOfWork.Current().FetchSession();
		protected IUnitOfWork UnitOfWork => CurrentUnitOfWork.Current();
		protected ICurrentUnitOfWork CurrentUnitOfWork;
		protected ICurrentBusinessUnit CurrentBusinessUnit;
		protected IDataSource DataSource;

		[SetUp]
		public void Setup()
		{
			var (person, businessUnit) = InfrastructureTestSetup.Setup();
			BusinessUnit = businessUnit;

			var builder = new ContainerBuilder();
			builder.RegisterModule(new CommonModule(new IocConfiguration(new IocArgs(new ConfigReader()) {FeatureToggle = "http://notinuse"})));
			builder.RegisterType<FakeLicenseRepository>().AsSelf().As<ILicenseRepository>().SingleInstance();
			builder.RegisterInstance(new FakeConfigReader().FakeInfraTestConfig()).AsSelf().As<IConfigReader>().SingleInstance();
			Container = builder.Build();

			CurrentUnitOfWork = Container.Resolve<ICurrentUnitOfWork>();
			CurrentBusinessUnit = Container.Resolve<ICurrentBusinessUnit>();
			DataSource = Container.Resolve<IDataSourceForTenant>().Tenant(InfraTestConfigReader.TenantName());

			Login(person, businessUnit);
			OpenUnitOfWork();
			SetupForRepositoryTest();
		}


		protected virtual void SetupForRepositoryTest()
		{
		}

		[TearDown]
		public void BaseTeardown()
		{
			EndUnitOfWork();
			Container.Dispose();
		}

		protected void PersistAndRemoveFromUnitOfWork(IEntity obj)
		{
			Session.SaveOrUpdate(obj);
			Session.Flush();
			Session.Evict(obj);
		}

		protected void OpenUnitOfWork()
		{
			DataSource.Application.CreateAndOpenUnitOfWork();
		}

		protected void EndUnitOfWork()
		{
			if (CurrentUnitOfWork.HasCurrent())
				CurrentUnitOfWork.Current()?.Dispose();
		}

		private void Login(IPerson person, IBusinessUnit businessUnit)
		{
			var principal = Container.Resolve<IPrincipalFactory>().MakePrincipal(new PersonAndBusinessUnit(person, businessUnit), DataSource, null);
			Container.Resolve<ICurrentPrincipalContext>().SetCurrentPrincipal(principal);
		}

		protected void Logout()
		{
			Container.Resolve<ICurrentPrincipalContext>().SetCurrentPrincipal(null);
		}
	}
}