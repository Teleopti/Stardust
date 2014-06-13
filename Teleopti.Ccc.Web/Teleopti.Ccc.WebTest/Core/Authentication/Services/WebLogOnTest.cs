using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
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
		private MockRepository mocks;
		private IDataSourcesProvider dataSourcesProvider;
		private IPersonRepository personRepository;
		private IBusinessUnitRepository businessUnitRepository;
		private ISessionSpecificDataProvider sessionSpecificDataProvider;
		private IRepositoryFactory repositoryFactory;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IPrincipalAuthorization principalAuthorization;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			dataSourcesProvider = mocks.DynamicMock<IDataSourcesProvider>();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			personRepository = mocks.DynamicMock<IPersonRepository>();
			businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();
			sessionSpecificDataProvider = mocks.StrictMock<ISessionSpecificDataProvider>(); //made this strict on purpose
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			logOnOff = mocks.DynamicMock<ILogOnOff>();
			var ruleToPrincipalCommand = mocks.DynamicMock<IRoleToPrincipalCommand>();
			principalAuthorization = mocks.DynamicMock<IPrincipalAuthorization>();
			ITeleoptiPrincipal principal = new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null), new Person());

			target = new WebLogOn(logOnOff,
			                      dataSourcesProvider,
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
			var choosenDatasource = mocks.DynamicMock<IDataSource>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var logonPerson = new Person();

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName))
					.Return(choosenDatasource);
				Expect.Call(choosenDatasource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);
				Expect.Call(repositoryFactory.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
				Expect.Call(personRepository.Get(personId)).Return(logonPerson);
				Expect.Call(businessUnitRepository.Get(buId)).Return(choosenBusinessUnit);
				Expect.Call(() => logOnOff.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
				Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(true);
				Expect.Call(() => sessionSpecificDataProvider.StoreInCookie(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.LogOn(dataSourceName, buId, personId);
			}
		}

		[Test]
		public void ShouldResolveIndataAndLogonWithAnywherePermission()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = mocks.DynamicMock<IDataSource>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var logonPerson = new Person();

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName))
					.Return(choosenDatasource);
				Expect.Call(choosenDatasource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);
				Expect.Call(repositoryFactory.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
				Expect.Call(personRepository.Get(personId)).Return(logonPerson);
				Expect.Call(businessUnitRepository.Get(buId)).Return(choosenBusinessUnit);
				Expect.Call(() => logOnOff.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
				Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(true);
				Expect.Call(() => sessionSpecificDataProvider.StoreInCookie(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.LogOn(dataSourceName, buId, personId);
			}
		}

		[Test]
		public void ShouldThrowIfNoMyTimeWebAndAnywherePermission()
		{
			var buId = Guid.NewGuid();
			const string dataSourceName = "sdfsjdlfkjsd ";
			var personId = Guid.NewGuid();

			var choosenBusinessUnit = new BusinessUnit("sdfsdf");
			var choosenDatasource = mocks.DynamicMock<IDataSource>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var logonPerson = new Person();

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName))
					.Return(choosenDatasource);
				Expect.Call(choosenDatasource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);
				Expect.Call(repositoryFactory.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
				Expect.Call(personRepository.Get(personId)).Return(logonPerson);
				Expect.Call(businessUnitRepository.Get(buId)).Return(choosenBusinessUnit);
				Expect.Call(() => logOnOff.LogOn(choosenDatasource, logonPerson, choosenBusinessUnit));
				Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)).Return(false);
				Expect.Call(principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere)).Return(false);
			}
			using (mocks.Playback())
			{
				Assert.Throws<PermissionException>(() =>
												   target.LogOn(dataSourceName, buId, personId));
			}
		}

	}
}