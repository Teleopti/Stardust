using System;
using System.Configuration;
using System.Web.Http;
using System.Xml.Linq;
using Autofac;
using Autofac.Integration.WebApi;
using NSwag.AspNet.Owin;
using Owin;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Api
{
	public class Startup
	{
		private readonly Action<IContainer> _afterContainerBuild;
		private readonly Action<ContainerBuilder> _optionalRegistrations;

		public Startup() : this(null,null)
		{
		}

		public Startup(Action<ContainerBuilder> optionalRegistrations, Action<IContainer> afterContainerBuild)
		{
			_afterContainerBuild = afterContainerBuild ?? (_ => {});
			_optionalRegistrations = optionalRegistrations ?? (_ => {});
		}

		public void Configuration(IAppBuilder app)
		{
			var container = configureContainer();
			_afterContainerBuild.Invoke(container);

			if (!StateHolderReader.IsInitialized)
			{
				var webSettings = new WebSettings
				{
					Settings = container.Resolve<ISharedSettingsQuerier>()
						.GetSharedSettings()
						.AddToAppSettings(ConfigurationManager.AppSettings.ToDictionary())
				};
				var settings = new WebConfigReader(() => webSettings);
				var messageBroker = container.Resolve<IMessageBrokerComposite>();
				var policy = settings.AppConfig("PasswordPolicy");
				var passwordPolicyService =
					new LoadPasswordPolicyService(string.IsNullOrEmpty(policy)
						? XDocument.Parse("<Root />")
						: XDocument.Parse(policy));
				var initializeApplication = new InitializeApplication(messageBroker);
				initializeApplication.Start(new State(), passwordPolicyService, webSettings.Settings);
				new InitializeMessageBroker(messageBroker).Start(webSettings.Settings);
			}

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

			app.UseCustomToken(container.Resolve<ITokenVerifier>(), container.Resolve<IRepositoryFactory>(),
				container.Resolve<ILogOnOff>(), container.Resolve<IAuthenticationQuerier>());

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
			builder.RegisterAssemblyTypes(typeof(Startup).Assembly).AsClosedTypesOf(typeof(IQueryHandler<,>)).ApplyAspects();
			builder.RegisterAssemblyTypes(typeof(Startup).Assembly).AsClosedTypesOf(typeof(ICommandHandler<>)).ApplyAspects();

			_optionalRegistrations.Invoke(builder);

			return builder.Build();
		}
	}
}