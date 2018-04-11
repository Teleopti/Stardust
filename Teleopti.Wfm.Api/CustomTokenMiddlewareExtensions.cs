using Owin;

namespace Teleopti.Wfm.Api
{
	public static class CustomTokenMiddlewareExtensions
	{
		public static IAppBuilder UseCustomToken(
			this IAppBuilder builder, ITokenVerifier tokenVerifier)
		{
			return builder.Use(typeof(TokenHandler), tokenVerifier);
		}
	}
}