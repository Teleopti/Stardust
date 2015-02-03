using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IAuthenticator, ISsoAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFindApplicationUser _findApplicationUser;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IFindApplicationUser findApplicationUser)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_findApplicationUser = findApplicationUser;
		}

		public AuthenticateResult AuthenticateWindowsUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindIdentityAuthenticatedPerson(winAccount.UserIdentifier,
					out foundUser))
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
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
				return new AuthenticateResult { DataSource = dataSource, Person = authResult.Person, Successful = authResult.Successful, HasMessage = authResult.HasMessage, Message = authResult.Message, PasswordExpired = authResult.PasswordExpired };
			}
		}
	}
}