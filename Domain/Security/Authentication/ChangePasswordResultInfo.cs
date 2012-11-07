using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class ChangePasswordResultInfo : IChangePasswordResultInfo
	{
		public bool IsSuccessful { get; set; }
		public bool IsAuthenticationSuccessful { get; set; }
	}
}
