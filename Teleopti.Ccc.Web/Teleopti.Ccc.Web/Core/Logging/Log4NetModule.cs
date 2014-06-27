using System;
using System.Collections;
using System.Globalization;
using System.Web;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Web.Core.Logging
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
		private readonly Action _configureLogging;
		private readonly Func<Exception> _getServerError;
		public const string LogMessageException = "Log4NetModule caught an unhandled exception";
		public const string LogMessage404 = "A 404 occurred";

		public Log4NetModule() : this(LogManager.GetLogger(typeof(Log4NetModule)), configureLogging, () => HttpContext.Current.Server.GetLastError()) { }

		private static void configureLogging()
		{
			var logName = "WebApps";
			var appId = "Web";

			if (HttpContext.Current.Request.ApplicationPath.EndsWith("Rta", true, CultureInfo.CurrentCulture))
			{
				logName = "RtaWebService";
				appId = "Rta";
			}

			if (HttpContext.Current.Request.ApplicationPath.EndsWith("Broker", true, CultureInfo.CurrentCulture))
			{
				logName = "WebBroker";
				appId = "Broker";
			}

			GlobalContext.Properties["logName"] = logName;
			GlobalContext.Properties["appId"] = appId;

			XmlConfigurator.Configure();
		}

		public Log4NetModule(ILog logger, Action configureLogging, Func<Exception> getServerError)
		{
			_logger = logger;
			_configureLogging = configureLogging;
			_getServerError = getServerError;
		}

		public void OnStart(HttpApplication application)
		{
			_configureLogging.Invoke();
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