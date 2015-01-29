using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class LogLogonAttempt : ILogLogonAttempt
	{
		private readonly ILoginAttemptModelFactory _loginAttemptModelFactory;
		private readonly IRepositoryFactory _repositoryFactory;

		public LogLogonAttempt(ILoginAttemptModelFactory loginAttemptModelFactory,
									IRepositoryFactory repositoryFactory)
		{
			_loginAttemptModelFactory = loginAttemptModelFactory;
			_repositoryFactory = repositoryFactory;
		}

		public void SaveAuthenticateResult(string userName, AuthenticateResult result)
		{
			var personId = result.Person == null ? null : result.Person.Id;
			var model = _loginAttemptModelFactory.Create(userName, personId, result.Successful);

			using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				_repositoryFactory.CreatePersonRepository(uow).SaveLoginAttempt(model);
				uow.PersistAll();
			}
		}
	}
}