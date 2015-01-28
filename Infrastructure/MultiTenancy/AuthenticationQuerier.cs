using System;
using System.Web;
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
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["username"] = userName;
			query["password"] = password;
			uriBuilder.Query = query.ToString();
			return uriBuilder.ExecuteJsonRequest<AuthenticationQueryResult>();
		}

		public AuthenticationQueryResult TryIdentityLogon(string identity)
		{
			var uriBuilder = new UriBuilder(_pathToTennantServer + "Tenant/IdentityLogon");
			var query = HttpUtility.ParseQueryString(uriBuilder.Query);
			query["identity"] = identity;
			uriBuilder.Query = query.ToString();
			return uriBuilder.ExecuteJsonRequest<AuthenticationQueryResult>();
		}
	}
}