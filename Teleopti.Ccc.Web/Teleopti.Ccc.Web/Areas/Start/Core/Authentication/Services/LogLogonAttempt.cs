using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class LogLogonAttempt : ILogLogonAttempt
	{
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IIpAddressResolver _ipAddressResolver;

		public LogLogonAttempt(ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IIpAddressResolver ipAddressResolver)
		{
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_ipAddressResolver = ipAddressResolver;
		}

		public void SaveAuthenticateResult(string userName, AuthenticateResult result)
		{
			var provider = "Application";
			if (string.IsNullOrEmpty(userName))
			{
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				userName = winAccount.UserIdentifier;
				provider = "Windows";
			}
			var model = new LoginAttemptModel
			{
				ClientIp = _ipAddressResolver.GetIpAddress(),
				Provider = provider,
				Client = "WEB",
				UserCredentials = userName,
				Result = result.Successful ? "LogonSuccess" : "LogonFailed"
			};
			if (result.Person != null) model.PersonId = result.Person.Id;
			using (var uow = result.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				_repositoryFactory.CreatePersonRepository(uow).SaveLoginAttempt(model);
				uow.PersistAll();
			}
		}
	}
}