using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly string _fullPathToFile;

		public AuthenticationFromFileQuerier(string fullPathToFile)
		{
			_fullPathToFile = fullPathToFile;
		}

		public AuthenticationQueryResult TryLogon(string userName, string password)
		{
			return readFile();
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity)
		{
			return readFile();
		}

		private AuthenticationQueryResult readFile()
		{
			if (!File.Exists(_fullPathToFile))
				return new AuthenticationQueryResult { DataSource = "", DataSourceEncrypted = "", FailReason = string.Format("No file with name {0}", _fullPathToFile), Success = false };

			string json = File.ReadAllText(_fullPathToFile);
			return JsonConvert.DeserializeObject<AuthenticationQueryResult>(json);
		}
	}
}