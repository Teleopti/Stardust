using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class WebLogOnTest
	{
		private IWebLogOn target;
		private ILogOnOff logOnOff;
		private IDataSourceForTenant dataSourceForTenant;
		private IPersonRepository personRepository;
		private IBusinessUnitRepository businessUnitRepository;
		private ISessionSpecificDataProvider sessionSpecificDataProvider;
		private IRepositoryFactory repositoryFactory;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IPrincipalAuthorization principalAuthorization;

		[SetUp]
		public void Setup()
		{
			dataSourceForTenant = MockRepository.GenerateMock<IDataSourceForTenant>();
			repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			sessionSpecificDataProvider = MockRepository.GenerateMock<ISessionSpecificDataProvider>();
			unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			logOnOff = MockRepository.GenerateMock<ILogOnOff>();
			var ruleToPrincipalCommand = MockRepository.GenerateMock<IRoleToPrincipalCommand>();
			principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			ITeleoptiPrincipal principal = new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null, null), new Person());

			target = new WebLogOn(logOnOff,
			                      dataSourceForTenant,
			                      repositoryFactory,
			                      sessionSpecificDataProvider,
			                      ruleToPrincipalCommand,
			                      new FakeCurrentTeleoptiPrincipal(principal),
								  principalAuthorization);
		}

		[Test]
		public void ShouldResolveIndataAndLogonWithMyTimeWebPermission()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = MockRepository.GenerateMock<IDataSource>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var logonPerson = new Person();

			dataSourceForTenant.Stub(x => x.Tenant(dataSourceName)).Return(choosenDatasource);
			choosenDatasource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
			repositoryFactory.Stub(x => x.CreateApplicationFunctionRepository(uow)).Return(MockRepository.GenerateMock<IApplicationFunctionRepository>());
			personRepository.Stub(x => x.Get(personId)).Return(logonPerson);
			businessUnitRepository.Stub(x => x.Get(buId)).Return(choosenBusinessUnit);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);

			target.LogOn(dataSourceName, buId, personId, null, true);

			logOnOff.AssertWasCalled(x => x.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
			sessionSpecificDataProvider.AssertWasCalled(x => x.StoreInCookie(null, true), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldResolveIndataAndLogonWithAnywherePermission()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = MockRepository.GenerateMock<IDataSource>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var logonPerson = new Person();

			dataSourceForTenant.Stub(x => x.Tenant(dataSourceName)).Return(choosenDatasource);
			choosenDatasource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
			repositoryFactory.Stub(x => x.CreateApplicationFunctionRepository(uow))
				.Return(MockRepository.GenerateMock<IApplicationFunctionRepository>());
			personRepository.Stub(x => x.Get(personId)).Return(logonPerson);
			businessUnitRepository.Stub(x => x.Get(buId)).Return(choosenBusinessUnit);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(true);
			
			target.LogOn(dataSourceName, buId, personId, null, false);

			logOnOff.AssertWasCalled(x => x.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
			sessionSpecificDataProvider.AssertWasCalled(x => x.StoreInCookie(null, false), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldThrowIfNoMyTimeWebAndAnywherePermission()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = MockRepository.GenerateMock<IDataSource>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var logonPerson = new Person();

			dataSourceForTenant.Stub(x => x.Tenant(dataSourceName)).Return(choosenDatasource);
			choosenDatasource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
			repositoryFactory.Stub(x => x.CreateApplicationFunctionRepository(uow))
				.Return(MockRepository.GenerateMock<IApplicationFunctionRepository>());
			personRepository.Stub(x => x.Get(personId)).Return(logonPerson);
			businessUnitRepository.Stub(x => x.Get(buId)).Return(choosenBusinessUnit);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(false);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(false);

			Assert.Throws<PermissionException>(() => target.LogOn(dataSourceName, buId, personId, null, false));

			logOnOff.AssertWasCalled(x => x.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
			sessionSpecificDataProvider.AssertWasNotCalled(x => x.StoreInCookie(null, false), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldRemoveAuthBridgeCookieAfterSaveWfmCookie()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = MockRepository.GenerateMock<IDataSource>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var logonPerson = new Person();

			dataSourceForTenant.Stub(x => x.Tenant(dataSourceName)).Return(choosenDatasource);
			choosenDatasource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
			repositoryFactory.Stub(x => x.CreateApplicationFunctionRepository(uow)).Return(MockRepository.GenerateMock<IApplicationFunctionRepository>());
			personRepository.Stub(x => x.Get(personId)).Return(logonPerson);
			businessUnitRepository.Stub(x => x.Get(buId)).Return(choosenBusinessUnit);
			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);

			target.LogOn(dataSourceName, buId, personId, null, true);

			sessionSpecificDataProvider.AssertWasCalled(x=>x.RemoveAuthBridgeCookie());
		}
	}
}