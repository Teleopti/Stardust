using System;
using System.Security.Principal;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Core.RequestContext.Initialize;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionPrincipalFactoryTest
	{
		private MockRepository mocks;
		private SessionPrincipalFactory target;
		private ISessionSpecificDataProvider sessionSpecificDataProvider;
		private IRepositoryFactory repositoryFactory;
		private IDataSourceForTenant dataSourceForTenant;
		private IRoleToPrincipalCommand roleToPrincipalCommand;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			sessionSpecificDataProvider = mocks.DynamicMock<ISessionSpecificDataProvider>();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			dataSourceForTenant = mocks.DynamicMock<IDataSourceForTenant>();
			roleToPrincipalCommand = mocks.DynamicMock<IRoleToPrincipalCommand>();

			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
		    target = new SessionPrincipalFactory(dataSourceForTenant, sessionSpecificDataProvider, repositoryFactory, roleToPrincipalCommand, new TeleoptiPrincipalFactory(), new TokenIdentityProvider(currentHttpContext));
		}

		[Test]
		public void ShouldCreatePrincipalWithCorrectData()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var sessData = createSessionData();
			var uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var personRepository = mocks.DynamicMock<IPersonRepository>();
			var person = new Person{Name=new Name("roger", "Moore")};
			var businessUnitRepository = mocks.DynamicMock<IBusinessUnitRepository>();
			var businessUnit = new BusinessUnit("sdf");

			using (mocks.Record())
			{
				Expect.Call(sessionSpecificDataProvider.GrabFromCookie()).Return(sessData);
				Expect.Call(dataSourceForTenant.Tenant(sessData.DataSourceName)).Return(dataSource);
				Expect.Call(dataSource.Application).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);
				Expect.Call(personRepository.Load(sessData.PersonId)).Return(person);
				Expect.Call(repositoryFactory.CreateBusinessUnitRepository(uow)).Return(businessUnitRepository);
				Expect.Call(businessUnitRepository.Get(sessData.BusinessUnitId)).Return(businessUnit);
			}
			using (mocks.Playback())
			{
				var principal = target.Generate();
				var identity = (ITeleoptiIdentity) principal.Identity;
				((IUnsafePerson)principal).Person.Should().Be.SameInstanceAs(person);
				identity.Name.Should().Be.EqualTo(person.Name.ToString());
				identity.DataSource.Should().Be.SameInstanceAs(dataSource);
				identity.BusinessUnit.Should().Be.SameInstanceAs(businessUnit);
				identity.WindowsIdentity.Name.Should().Be.EqualTo(WindowsIdentity.GetCurrent().Name);
			}
		}

		[Test]
		public void ShouldReturnNullIfSessionDataIsNotAvailable()
		{
			using (mocks.Record())
			{
				Expect.Call(sessionSpecificDataProvider.GrabFromCookie()).Return(null);
			}
			using (mocks.Playback())
			{
				target.Generate().Should().Be.Null();
			}
		}

		[Test]
		public void ShouldReturnNullIfSessionDataPointsToNonExistingDatabase()
		{
			var sessionData = new SessionSpecificData(Guid.NewGuid(), "sdf", Guid.NewGuid(), RandomName.Make());
			using (mocks.Record())
			{
				Expect.Call(sessionSpecificDataProvider.GrabFromCookie()).Return(sessionData);
			}
			using (mocks.Playback())
			{
				target.Generate().Should().Be.Null();
			}
		}

		private static SessionSpecificData createSessionData()
		{
			return new SessionSpecificData(Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid(), RandomName.Make());
		}
	}
}