using Owin;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Wfm.Api
{
	public static class CustomTokenMiddlewareExtensions
	{
		public static IAppBuilder UseCustomToken(
			this IAppBuilder builder, ITokenVerifier tokenVerifier, IDataSourceForTenant dataSourceForTenant, ILoadUserUnauthorized loadUserUnauthorized, IRepositoryFactory repositoryFactory, ILogOnOff logOnOff)
		{
			return builder.Use(typeof(TokenHandler), tokenVerifier, dataSourceForTenant, loadUserUnauthorized, repositoryFactory, logOnOff);
		}
	}
}