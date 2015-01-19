using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IMultiTenancyWindowsLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel);
	}

	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;

		public MultiTenancyWindowsLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier,
			IWindowsUserProvider windowsUserProvider)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
		}

		public AuthenticationResult Logon(ILogonModel logonModel)
		{
			throw new System.NotImplementedException();
		}
	}
}