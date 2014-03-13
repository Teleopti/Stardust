using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
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
		private ITokenIdentityProvider tokenIdentityProvider;
		private IFindApplicationUser findApplicationUser;
		private IIpAddressResolver ipFinder;
		const string dataSourceName = "Gurkmajonääääs";

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			dataSourcesProvider = mocks.StrictMock<IDataSourcesProvider>();
			repositoryFactory = mocks.DynamicMock<IRepositoryFactory>();
			tokenIdentityProvider = mocks.DynamicMock<ITokenIdentityProvider>();
			findApplicationUser = mocks.DynamicMock<IFindApplicationUser>();
			ipFinder = mocks.DynamicMock<IIpAddressResolver>();
			target = new Authenticator(dataSourcesProvider, tokenIdentityProvider, repositoryFactory, findApplicationUser, ipFinder);
		}

		
		[Test]
		public void AuthenticateWindowsUserShouldReturnSuccessfulAuthenticationResult()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var personRepository = mocks.DynamicMock<IPersonRepository>();
			var winAccount = new TokenIdentity {UserDomain = "domain", UserIdentifier = "user"};

			IPerson person;

			using (mocks.Record())
			{
				Expect.Call(dataSourcesProvider.RetrieveDataSourceByName(dataSourceName)).Return(dataSource);
				Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);

				Expect.Call(tokenIdentityProvider.RetrieveToken()).Return(winAccount);

				Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRepository);

				Expect.Call(personRepository.TryFindWindowsAuthenticatedPerson(winAccount.UserDomain, winAccount.UserIdentifier, out person)).Return(true);
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
			var domainAuthResult = new AuthenticationResult { HasMessage = true, Message = "sdf", Person = new Person(), Successful = true , PasswordExpired = true};
			

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
				result.PasswordExpired = domainAuthResult.PasswordExpired;
				result.Successful = domainAuthResult.Successful;
			}
		}

		[Test]
		public void ShouldSaveAuthenticateResult()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var personRep = mocks.DynamicMock<IPersonRepository>();
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var result = new AuthenticateResult { Successful = false,DataSource = dataSource};

			var model = new LoginAttemptModel
				{
					ClientIp ="",
					Provider = "Application",
					UserCredentials = "hej",
					Result = "LogonSuccess"
				};
			
			Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
			Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(ipFinder.GetIpAddress()).IgnoreArguments().Return("");
			Expect.Call(repositoryFactory.CreatePersonRepository(uow)).Return(personRep);
			Expect.Call(personRep.SaveLoginAttempt(model)).IgnoreArguments().Return(1);
			mocks.ReplayAll();
			target.SaveAuthenticateResult("hej", result);
			mocks.VerifyAll();
		}

		
	}
}