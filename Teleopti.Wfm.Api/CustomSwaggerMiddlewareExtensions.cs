using Owin;

namespace Teleopti.Wfm.Api
{
	public static class CustomSwaggerMiddlewareExtensions
	{
		public static IAppBuilder UseCustomSwagger(
			this IAppBuilder builder)
		{
			return builder.Use(typeof(SwaggerHandler), new CommandDtoProvider(), new QueryHandlerProvider());
		}
	}
}