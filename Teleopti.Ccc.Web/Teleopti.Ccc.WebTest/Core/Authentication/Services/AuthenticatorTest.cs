using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	[TestFixture]
	public class AuthenticatorTest
	{
		private IDataSourcesProvider dataSourcesProvider;
		private IRepositoryFactory repositoryFactory;
		private MockRepository mocks;
		private IAuthenticator target;
		private IWindowsAccountProvider windowsAccountProvider;
		private IFindApplicationUser findApplicationUser;
		const string dataSourceName = "Gurkmajonääääs";

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			dataSourcesProvider = mocks.StrictMock<IDataSourcesProvider>();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			windowsAccountProvider = mocks.DynamicMock<IWindowsAccountProvider>();
			findApplicationUser = mocks.DynamicMock<IFindApplicationUser>();

			target = new Authenticator(dataSourcesProvider, windowsAccountProvider, repositoryFactory, findApplicationUser);
		}


		[Test]
		public void AuthenticateWindowsUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var personRepository = mocks.DynamicMock<IPersonRepository>();
			var winAccount = new WindowsAccount("domain", "user");

			IPerson person;

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName)).Return(dataSource);
				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

				Expect.Call(windowsAccountProvider.RetrieveWindowsAccount()).Return(winAccount);

				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);

				Expect.Call(personRepository.TryFindWindowsAuthenticatedPerson(winAccount.DomainName, winAccount.UserName, out person)).Return(true);
			}

			using (mocks.Playback())
			{
				var result = target.AuthenticateWindowsUser(dataSourceName);
				result.Person.Should().Be.EqualTo(person);
				result.Successful.Should().Be.True();
				result.DataSource.Should().Be.SameInstanceAs(dataSource);
			}
		}

		[Test]
		public void AuthenticateApplicationUserShouldReturnAuthenticationResult()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var domainAuthResult = new AuthenticationResult { HasMessage = true, Message = "sdf", Person = new Person(), Successful = true };
			

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName)).Return(dataSource);
				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

				Expect.Call(findApplicationUser.CheckLogOn(uow, "logon name", "password"))
					.Return(domainAuthResult);
			}

			using (mocks.Playback())
			{
				var result = target.AuthenticateApplicationUser(dataSourceName, "logon name", "password");
				result.DataSource.Should().Be.SameInstanceAs(dataSource);
				result.HasMessage = domainAuthResult.HasMessage;
				result.Message = domainAuthResult.Message;
				result.Person = domainAuthResult.Person;
				result.Successful = domainAuthResult.Successful;
			}
		}
	}
}