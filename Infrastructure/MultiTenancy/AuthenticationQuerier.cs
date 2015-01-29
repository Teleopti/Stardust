using System;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	//tested by web scenarios
	public class AuthenticationQuerier : IAuthenticationQuerier
	{
		private readonly string _pathToTennantServer;

		public AuthenticationQuerier(string pathToTennantServer)
		{
			_pathToTennantServer = pathToTennantServer;
		}

		public AuthenticationQueryResult TryLogon(string userName, string password)
		{
			var uriBuilder = new UriBuilder(_pathToTennantServer + "Tenant/ApplicationLogon");
			var post = string.Format("userName={0}&password={1}", userName, password);

			return uriBuilder.PostRequest<AuthenticationQueryResult>(post);
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity)
		{
			var uriBuilder = new UriBuilder(_pathToTennantServer + "Tenant/IdentityLogon");
			var post = string.Format("identity={0}", identity);

			return uriBuilder.PostRequest<AuthenticationQueryResult>(post);
		}
	}
}