using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Ccc.Web.Areas.SSO.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Toggle;
using Teleopti.Ccc.Web.Broker;
using Teleopti.Ccc.Web.Core.Logging;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(10)]
	public class RegisterGlobalApiFiltersTask : IBootstrapperTask
	{
		private readonly IGlobalConfiguration _globalConfiguration;
		private readonly Log4NetLogger _log4NetLogger;
		private readonly IConfigReader _configReader;

		public RegisterGlobalApiFiltersTask(IGlobalConfiguration globalConfiguration, Log4NetLogger log4NetLogger, IConfigReader configReader)
		{
			_globalConfiguration = globalConfiguration;
			_log4NetLogger = log4NetLogger;
			_configReader = configReader;
		}

		public Task Execute(IAppBuilder application)
		{
			_globalConfiguration.Configure(c =>
			{
				c.Filters.Add(new AuthorizeTeleoptiAttribute(new[]
				{
					typeof (MessageBrokerController),
					typeof (ToggleHandlerController),
					typeof (AuthenticateController),
					typeof (PersonInfoController),
					typeof (ApplicationAuthenticationApiController),
					typeof (AuthenticationApiController),
					typeof (StateController),
					typeof (ConfigurationController),
					typeof (LanguageController),
					typeof (ConfigController),
					typeof (JavascriptLoggingController),
					typeof (PasswordController),
					typeof (ExternalApplicationAccessController)
				}));

				if (string.IsNullOrEmpty(_configReader.AppConfig("DisableCsrfProtection")))
				{
					c.Filters.Add(new CsrfFilterHttp());
				}
				c.Filters.Add(new NoCacheFilterHttp());
				c.Services.Add(typeof (IExceptionLogger), new Log4NetWebApiLogger(_log4NetLogger));
				
				c.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver();
				c.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
				c.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new DateOnlyConverter());
			});
			
			return Task.FromResult(false);
		}
	}
}