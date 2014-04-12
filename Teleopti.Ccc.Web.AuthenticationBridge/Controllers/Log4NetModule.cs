using System;
using System.Collections;
using System.Web;
using log4net;
using log4net.Config;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.AuthenticationBridge.Controllers
{
	public class Log4NetModule : IHttpModule
	{
		private static bool _applicationStarted;
		private static readonly object _applicationStartLock = new object();

		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			if (!_applicationStarted)
			{
				lock (_applicationStartLock)
				{
					if (!_applicationStarted)
					{
						// this will run only once per application start
						OnStart(application);
						_applicationStarted = true;
					}
				}
			}
			// this will run on every HttpApplication initialization in the application pool
			OnInit(application);
		}

		private readonly ILog _logger;
		private readonly Func<ICollection> _configureLogging;
		private readonly Func<Exception> _getServerError;
		public const string LogMessageException = "Log4NetModule caught an unhandled exception";
		public const string LogMessage404 = "A 404 occurred";

		public Log4NetModule() : this(LogManager.GetLogger(typeof(Log4NetModule)), XmlConfigurator.Configure, () => HttpContext.Current.Server.GetLastError()) { }

		public Log4NetModule(ILog logger, Func<ICollection> configureLogging, Func<Exception> getServerError)
		{
			_logger = logger;
			_configureLogging = configureLogging;
			_getServerError = getServerError;
		}

		public void OnStart(HttpApplication application)
		{
			_configureLogging.Invoke();
			FederatedAuthentication.ServiceConfiguration.SecurityTokenHandlers.AddOrReplace(new MachineKeySessionSecurityTokenHandler());
		}

		public void OnInit(HttpApplication application)
		{
			application.Error += Application_Error;
		}

		public void Application_Error(object sender, EventArgs e) { LogException(_getServerError.Invoke()); }

		private void LogException(Exception exception)
		{
			var innerMostException = GetInnerMostException(exception);

			if (innerMostException is HttpException && ((HttpException) innerMostException).GetHttpCode() == 404)
				_logger.Warn(LogMessage404, innerMostException);
			else
				_logger.Error(LogMessageException, innerMostException);
		}

		private static Exception GetInnerMostException(Exception exception)
		{
			while (exception.InnerException != null)
				exception = exception.InnerException;
			return exception;
		}
	}
}