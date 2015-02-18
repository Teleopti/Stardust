using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class TenantSsoAuthenticator : ISsoAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IApplicationAuthentication _applicationAuthentication;

		public TenantSsoAuthenticator(IDataSourcesProvider dataSourceProvider,
																IRepositoryFactory repositoryFactory,
																IApplicationAuthentication applicationAuthentication)
		{
			_dataSourceProvider = dataSourceProvider;
			_repositoryFactory = repositoryFactory;
			_applicationAuthentication = applicationAuthentication;
		}

		public AuthenticateResult AuthenticateApplicationUser(string dataSourceName, string userName, string password)
		{
			var result = _applicationAuthentication.Logon(userName, password);

			if (result.Success)
			{
				var dataSource = _dataSourceProvider.RetrieveDataSourceByName(result.Tenant);
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(result.PersonId); //todo: don't load permissions here - just needed to get web scenarios to work
					return new AuthenticateResult { DataSource = dataSource, Person = person, Successful = true, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = false };
				}
			}

			return new AuthenticateResult { DataSource = null, Person = null, Successful = false, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = result.PasswordExpired };
		}
	}
}