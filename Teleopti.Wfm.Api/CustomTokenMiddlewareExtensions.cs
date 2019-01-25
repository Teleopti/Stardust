using Owin;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Wfm.Api
{
	public static class CustomTokenMiddlewareExtensions
	{
		public static IAppBuilder UseCustomToken(
			this IAppBuilder builder, ITokenVerifier tokenVerifier, IRepositoryFactory repositoryFactory, ILogOnOff logOnOff, IAuthenticationTenantClient authenticationQuerier)
		{
			return builder.Use(typeof(TokenHandler), tokenVerifier, repositoryFactory, logOnOff, authenticationQuerier);
		}
	}
}