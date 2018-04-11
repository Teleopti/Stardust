using System;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using NSwag.AspNet.Owin;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Wfm.Api
{
	public class Startup
	{
		private readonly Action<ContainerBuilder> _optionalRegistrations;

		public Startup(Action<ContainerBuilder> optionalRegistrations = null)
		{
			_optionalRegistrations = optionalRegistrations ?? (_ => {});
		}

		public void Configuration(IAppBuilder app)
		{
			var container = configureContainer();
			
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

			app.UseCustomToken(container.Resolve<ITokenVerifier>());

			app.UseWebApi(httpConfiguration);
		}

		private IContainer configureContainer()
		{
			var builder = new ContainerBuilder();

			var args = new IocArgs(new ConfigReader())
			{
				DataSourceConfigurationSetter = DataSourceConfigurationSetter.ForApi()
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));

			builder.RegisterModule(new CommonModule(configuration));
			builder.RegisterType<CommandDtoProvider>();
			builder.RegisterType<QueryDtoProvider>();
			builder.RegisterType<DtoProvider>();
			builder.RegisterType<QueryHandlerProvider>();
			builder.RegisterType<TokenVerifier>().As<ITokenVerifier>();
			builder.RegisterApiControllers(typeof(Startup).Assembly);

			_optionalRegistrations.Invoke(builder);

			return builder.Build();
		}
	}
}