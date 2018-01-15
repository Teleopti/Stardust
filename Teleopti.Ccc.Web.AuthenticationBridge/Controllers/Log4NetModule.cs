using System;
using System.Collections;
using System.IdentityModel.Services;
using System.Web;
using log4net;
using log4net.Config;

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
		public const string LogMessageExceptionOuter = "Log4NetModule caught an unhandled exception (outer)";
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
			FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Name = "FedAuth45";
			FederatedAuthentication.FederationConfiguration.IdentityConfiguration.SecurityTokenHandlers.AddOrReplace(new MachineKeySessionSecurityTokenHandler());
		}

		public void OnInit(HttpApplication application)
		{
			application.Error += Application_Error;
			application.BeginRequest += (sender, args) =>
			{
				FederatedAuthentication.SessionAuthenticationModule.CookieHandler.Name = "FedAuth45";
			};
		}

		public void Application_Error(object sender, EventArgs e)
		{
			var exception = _getServerError.Invoke();
			LogException(exception);
		}

		private void LogException(Exception exception)
		{
			var httpException = GetInnerMostException(exception) as HttpException;
			if (httpException != null && httpException.GetHttpCode() == 404)
				_logger.Warn(LogMessage404, exception);
			else if (httpException != null && httpException.ErrorCode == unchecked((int) 0x80070057))
			{
				// ignore “The remote host closed the connection” exception
			}
			else
				_logger.Error(LogMessageException, exception);
		}

		private static Exception GetInnerMostException(Exception exception)
		{
			while (exception.InnerException != null)
				exception = exception.InnerException;
			return exception;
		}
	}
}