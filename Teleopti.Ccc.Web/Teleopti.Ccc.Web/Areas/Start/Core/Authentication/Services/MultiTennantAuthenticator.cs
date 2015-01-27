using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Tennant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class MultiTennantAuthenticator : IAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly IIdentityAuthentication _identityAuthentication;

		public MultiTennantAuthenticator(IDataSourcesProvider dataSourceProvider,
			ITokenIdentityProvider tokenIdentityProvider,
			IRepositoryFactory repositoryFactory,
			IApplicationAuthentication applicationAuthentication,
			IIdentityAuthentication identityAuthentication)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_applicationAuthentication = applicationAuthentication;
			_identityAuthentication = identityAuthentication;
		}

		public AuthenticateResult AuthenticateWindowsUser(string dataSourceName)
		{
			var winAccount = _tokenIdentityProvider.RetrieveToken();
			var result = _identityAuthentication.Logon(winAccount.UserIdentifier);
			if (result.Success)
			{
				IDataSource dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(result.PersonId);
					return new AuthenticateResult { DataSource = dataSource, Person = person, Successful = true, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = false };

				}
			}

			return new AuthenticateResult { DataSource = null, Person = null, Successful = false, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = false };
			
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
			var result = _applicationAuthentication.Logon(userName, password);

			if (result.Success)
			{
				var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(result.PersonId);
					return new AuthenticateResult { DataSource = dataSource, Person = person, Successful = true, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = false };
				}
			}

			return new AuthenticateResult { DataSource = null, Person = null, Successful = false, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = result.PasswordExpired };
		}

		public void SaveAuthenticateResult(string userName, AuthenticateResult result)
		{
			//dont do anything here it will happen in the new service

			//var provider = "Application";
			//if (string.IsNullOrEmpty(userName))
			//{
			//	var winAccount = _tokenIdentityProvider.RetrieveToken();
			//	userName = winAccount.UserIdentifier;
			//	provider = "Windows";
			//}
			//using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			//{
			//	var model = new LoginAttemptModel
			//	{
			//		ClientIp = _ipAddressResolver.GetIpAddress(),
			//		Provider = provider,
			//		Client = "WEB",
			//		UserCredentials = userName,
			//		Result = result.Successful ? "LogonSuccess" : "LogonFailed"
			//	};
			//	if (result.Person != null) model.PersonId = result.Person.Id;

			//	_repositoryFactory.CreatePersonRepository(uow).SaveLoginAttempt(model);
			//	uow.PersistAll();
			//}
		}

		
	}
}