using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using NSwag.AspNet.Owin;
using Owin;

namespace Teleopti.Wfm.Api
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var x = new ContainerBuilder();
			x.RegisterType<CommandDtoProvider>();
			x.RegisterType<QueryDtoProvider>();
			x.RegisterType<DtoProvider>();
			x.RegisterType<QueryHandlerProvider>();
			x.RegisterType<HashWrapper>();
			x.RegisterType<TokenVerifier>();
			x.RegisterApiControllers(typeof(Startup).Assembly);
			var container = x.Build();
			
			var swaggerUiSettings = new SwaggerUi3Settings
			{
				DefaultPropertyNameHandling = NJsonSchema.PropertyNameHandling.CamelCase,
				SwaggerRoute = "/swagger/v1/swagger.json"
			};

			var httpConfiguration = new HttpConfiguration();
			httpConfiguration.MapHttpAttributeRoutes();

			app.UseCustomSwagger();

			app.UseAutofacMiddleware(container);
			app.UseAutofacLifetimeScopeInjector(container);
			app.UseAutofacWebApi(httpConfiguration);

			app.UseSwaggerUi3(swaggerUiSettings);

			app.UseCustomToken();

			app.UseWebApi(httpConfiguration);
		}
	}
}