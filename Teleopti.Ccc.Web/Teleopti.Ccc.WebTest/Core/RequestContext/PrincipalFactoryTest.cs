using System;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	//ouch - för stor. blame roger. fixar till senare
	//MS: I think this needs to be split into several resposibilites: data provider(s?), principal factory at the least..
	[TestFixture]
	public class PrincipalFactoryTest
	{
		private MockRepository mocks;
		private SessionPrincipalFactory target;
		private ISessionSpecificDataProvider sessionSpecificDataProvider;
		private IRepositoryFactory repositoryFactory;
		private IDataSourcesProvider dataSourcesProvider;
		private IRoleToPrincipalCommand roleToPrincipalCommand;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			sessionSpecificDataProvider = mocks.DynamicMock<ISessionSpecificDataProvider>();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			dataSourcesProvider = mocks.DynamicMock<IDataSourcesProvider>();
			roleToPrincipalCommand = mocks.DynamicMock<IRoleToPrincipalCommand>();
			target = new SessionPrincipalFactory(dataSourcesProvider, sessionSpecificDataProvider, repositoryFactory, roleToPrincipalCommand);
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
				Expect.Call(sessionSpecificDataProvider.Grab()).Return(sessData);
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(sessData.DataSourceName)).Return(dataSource);
				Expect.Call(dataSource.Application).Return(uowFactory);
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);
				Expect.Call(personRepository.Get(sessData.PersonId)).Return(person);
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
				identity.TeleoptiAuthenticationType.Should().Be.EqualTo(sessData.AuthenticationType);
			}
		}

		[Test]
		public void ShouldReturnNullIfSessionDataIsNotAvailable()
		{
			using (mocks.Record())
			{
				Expect.Call(sessionSpecificDataProvider.Grab()).Return(null);
			}
			using (mocks.Playback())
			{
				target.Generate().Should().Be.Null();
			}
		}

		[Test]
		public void ShouldReturnNullIfSessionDataPointsToNonExistingDatabase()
		{
			var sessionData = new SessionSpecificData(Guid.NewGuid(), "sdf", Guid.NewGuid(), AuthenticationTypeOption.Windows);
			using (mocks.Record())
			{
				Expect.Call(sessionSpecificDataProvider.Grab()).Return(sessionData);
			}
			using (mocks.Playback())
			{
				target.Generate().Should().Be.Null();
			}
		}

		private static SessionSpecificData createSessionData()
		{
			return new SessionSpecificData(Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid(), AuthenticationTypeOption.Windows);
		}
	}
}