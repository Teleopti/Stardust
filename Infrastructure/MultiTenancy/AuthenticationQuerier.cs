using System;
using System.Net;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	//tested by web scenarios
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly string _pathToTenantServer;

		public AuthenticationQuerier(string pathToTenantServer)
		{
			_pathToTenantServer = pathToTenantServer;
		}

		public AuthenticationQueryResult TryLogon(string userName, string password, string userAgent)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/ApplicationLogon");
			var post = string.Format("userName={0}&password={1}", userName, password);

			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.UserAgent = userAgent;

			return request.PostRequest<AuthenticationQueryResult>(post);
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity, string userAgent)
		{
			var uriBuilder = new UriBuilder(_pathToTenantServer + "Tenant/IdentityLogon");
			var post = string.Format("identity={0}", identity);

			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.UserAgent = userAgent;

			return request.PostRequest<AuthenticationQueryResult>(post);
		}
	}
}