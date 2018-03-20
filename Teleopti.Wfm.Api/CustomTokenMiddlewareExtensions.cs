using Owin;

namespace Teleopti.Wfm.Api
{
	public static class CustomTokenMiddlewareExtensions
	{
		public static IAppBuilder UseCustomToken(
			this IAppBuilder builder)
		{
			return builder.Use(typeof(TokenHandler), new TokenVerifier(new HashWrapper()));
		}
	}
}