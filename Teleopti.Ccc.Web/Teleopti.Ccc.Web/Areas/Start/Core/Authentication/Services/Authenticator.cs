using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFindApplicationUser _findApplicationUser;
		private readonly IIpAddressResolver _ipAddressResolver;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IFindApplicationUser findApplicationUser,
									IIpAddressResolver ipAddressResolver)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_findApplicationUser = findApplicationUser;
			_ipAddressResolver = ipAddressResolver;
		}

		public AuthenticateResult AuthenticateWindowsUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindIdentityAuthenticatedPerson(winAccount.OriginalToken,
					out foundUser))
				{
					return new AuthenticateResult {Successful = true, Person = foundUser, DataSource = dataSource};
				}
			}

			return null;
		}

		public AuthenticateResult AuthenticateApplicationIdentityUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var account = _tokenIdentityProvider.RetrieveToken();
				var foundUser = _repositoryFactory.CreatePersonRepository(uow).TryFindBasicAuthenticatedPerson(account.UserIdentifier);
				if (foundUser != null)
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
				}
			}

			return null;
		}

		public AuthenticateResult AuthenticateApplicationUser(string dataSourceName, string userName, string password)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var authResult = _findApplicationUser.CheckLogOn(uow, userName, password);
				uow.PersistAll();
				return new AuthenticateResult {DataSource = dataSource, Person = authResult.Person, Successful = authResult.Successful, HasMessage = authResult.HasMessage, Message = authResult.Message, PasswordExpired = authResult.PasswordExpired};
			}
		}

		public void SaveAuthenticateResult(string userName, AuthenticateResult result)
		{
			var provider = "Application";
			if (string.IsNullOrEmpty(userName))
			{
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				userName = winAccount.UserDomain + "\\" + winAccount.UserIdentifier;
				provider = "Windows";
			}
			using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var model = new LoginAttemptModel
				{
					ClientIp = _ipAddressResolver.GetIpAddress(),
					Provider = provider,
					Client = "WEB",
					UserCredentials = userName,
					Result = result.Successful ? "LogonSuccess" : "LogonFailed"
				};
				if (result.Person != null) model.PersonId = result.Person.Id;

				_repositoryFactory.CreatePersonRepository(uow).SaveLoginAttempt(model);
				uow.PersistAll();
			}
		}

		
	}
}