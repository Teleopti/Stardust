using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly string _fullPathToFile;

		public AuthenticationFromFileQuerier(string fullPathToFile)
		{
			_fullPathToFile = fullPathToFile;
		}

		public AuthenticationQueryResult TryApplicationLogon(string userName, string password, string userAgent)
		{
			return readFile();
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent)
		{
			return readFile();
		}

		private AuthenticationQueryResult readFile()
		{
			if (!File.Exists(_fullPathToFile))
				return new AuthenticationQueryResult { FailReason = string.Format("No file with name {0}", _fullPathToFile), Success = false };

			var json = File.ReadAllText(_fullPathToFile);
			var ret = JsonConvert.DeserializeObject<AuthenticationQueryResult>(json);
			ret.PasswordPolicy =
				"<!--Default config data-->\r\n<PasswordPolicy MaxNumberOfAttempts=\"3\" InvalidAttemptWindow=\"0\" PasswordValidForDayCount=\"2147483647\" PasswordExpireWarningDayCount=\"0\" />";
			return ret;
		}
	}
}