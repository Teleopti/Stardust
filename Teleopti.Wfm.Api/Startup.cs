using System.Configuration;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Xml.Linq;
using Autofac;
using log4net.Config;
using NSwag.AspNet.Owin;
using Owin;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.IocCommon;


namespace Teleopti.Wfm.Api
{
	public class Startup
	{
		private readonly ILifetimeScope _container;

		public Startup() : this(null)
		{
		}

		public Startup(ILifetimeScope container)
		{
			_container = container;
		}

		public void Configuration(IAppBuilder app)
		{
			XmlConfigurator.Configure();
			var container = _container ?? configureContainer();
			
			if (!StateHolderReader.IsInitialized)
			{
				var webSettings = new WebSettings
				{
					Settings = container.Resolve<ISharedSettingsTenantClient>()
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
			httpConfiguration.Services.Replace(typeof(IAssembliesResolver), new SlimAssembliesResolver(typeof(SlimAssembliesResolver).Assembly));
			httpConfiguration.MapHttpAttributeRoutes();

			app.UseCustomSwagger();

			app.UseAutofacMiddleware(container);
			app.UseAutofacLifetimeScopeInjector(container);
			app.UseAutofacWebApi(httpConfiguration);

			app.UseSwaggerUi3(swaggerUiSettings);

			app.UseCustomToken(container.Resolve<ITokenVerifier>(), container.Resolve<IRepositoryFactory>(),
				container.Resolve<ILogOnOff>(), container.Resolve<IAuthenticationTenantClient>());

			app.UseWebApi(httpConfiguration);
			container.Resolve<IHangfireClientStarter>().Start();
		}

		private IContainer configureContainer()
		{
			var builder = new ContainerBuilder();

			var args = new IocArgs(new ConfigReader())
			{
				DataSourceApplicationName = DataSourceApplicationName.ForApi(),
				TeleoptiPrincipalForLegacy = true
			};
			var configuration = new IocConfiguration(args, CommonModule.ToggleManagerForIoc(args));
			builder.RegisterModule(new ApiModule(configuration));
			builder.RegisterModule(new CommonModule(configuration));

			return builder.Build();
		}
	}
}