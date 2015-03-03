using System;
using System.Net;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	//tested by web scenarios
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly string _pathToTenantServer;
		private readonly INhibConfigEncryption _nhibConfigEncryption;

		public AuthenticationQuerier(string pathToTenantServer, INhibConfigEncryption nhibConfigEncryption)
		{
			_pathToTenantServer = pathToTenantServer;
			_nhibConfigEncryption = nhibConfigEncryption;
		}

		public AuthenticationQueryResult TryLogon(string userName, string password, string userAgent)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/ApplicationLogon");
			var post = string.Format("userName={0}&password={1}", userName, password);

			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.UserAgent = userAgent;

			var answer = request.PostRequest<AuthenticationQueryResult>(post);
			answer.DataSourceConfiguration = _nhibConfigEncryption.DecryptConfig(answer.DataSourceConfiguration);
			return answer;
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/IdentityLogon");
			var post = string.Format("identity={0}", identity);

			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.UserAgent = userAgent;

			var answer = request.PostRequest<AuthenticationQueryResult>(post);
			answer.DataSourceConfiguration = _nhibConfigEncryption.DecryptConfig(answer.DataSourceConfiguration);
			return answer;
		}
	}
}