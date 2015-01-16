using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Domain.Security
{
	public class ApplicationLogon : IApplicationLogon
	{
		public AuthenticationResult Logon(ILogonModel logonModel)
		{
			return logonModel.SelectedDataSourceContainer.LogOn(logonModel.UserName, logonModel.Password);
		}
		public bool ShowDataSourceSelection { get { return true; } }
	}
}