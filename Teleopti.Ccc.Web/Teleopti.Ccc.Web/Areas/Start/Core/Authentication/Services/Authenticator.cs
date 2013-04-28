using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFindApplicationUser _findApplicationUser;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									IWindowsAccountProvider windowsAccountProvider,
									IRepositoryFactory repositoryFactory,
									IFindApplicationUser findApplicationUser)
		{
			_dataSourceProvider = dataSourceProvider;
			_windowsAccountProvider = windowsAccountProvider;
			_repositoryFactory = repositoryFactory;
			_findApplicationUser = findApplicationUser;
		}


		public AuthenticateResult AuthenticateWindowsUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				var winAccount = _windowsAccountProvider.RetrieveWindowsAccount();
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindWindowsAuthenticatedPerson(winAccount.DomainName,
																	winAccount.UserName,
																	out foundUser))
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
				var winAccount = _windowsAccountProvider.RetrieveWindowsAccount();
				userName = winAccount.DomainName + "\\" + winAccount.UserName;
				provider = "Windows";
			}
			using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var model = new LoginAttemptModel
				{
					ClientIp = ipAddress(),
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

		private string ipAddress()
		{
			// in test environment
			if (System.Web.HttpContext.Current != null)
				return System.Web.HttpContext.Current.Request.UserHostAddress;

			return "0.0.0.0";
			//var context = System.Web.HttpContext.Current;
			//var sIpAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

			//if(string.IsNullOrEmpty(sIpAddress))
			//	return context.Request.ServerVariables["REMOTE_ADDR"];

			//var ipArray = sIpAddress.Split(',');

			//return ipArray[0];

		}


	}
}