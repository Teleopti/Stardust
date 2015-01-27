using System.IO;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class AuthenticationFromFileQuerier : IAuthenticationQuerier
	{
		private readonly string _pathToFile;

		public AuthenticationFromFileQuerier(string pathToFile)
		{
			_pathToFile = pathToFile;
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
			var file = _pathToFile + "Authentication.json";
			if (!File.Exists(file))
				return new AuthenticationQueryResult { DataSource = "", DataSourceEncrypted = "", FailReason = string.Format("No file with name {0}", file), Success = false };

			string json = File.ReadAllText(file);
			return JsonConvert.DeserializeObject<AuthenticationQueryResult>(json);
		}
	}
}