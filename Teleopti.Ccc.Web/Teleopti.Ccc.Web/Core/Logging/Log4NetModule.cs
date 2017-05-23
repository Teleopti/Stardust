using System;
using System.Globalization;
using System.Web;
using log4net;
using log4net.Config;

namespace Teleopti.Ccc.Web.Core.Logging
{
	public class Log4NetModule : IHttpModule
	{
		public const string LogMessageException = "Log4NetModule caught an unhandled exception";
		public const string LogMessage404 = "A 404 occurred";
		private static readonly object applicationStartLock = new object();
		private static bool _applicationStarted;
		private readonly Action _configureLogging;
		private readonly Func<Exception> _getServerError;
		private readonly Log4NetLogger _log4NetLogger;

		public Log4NetModule()
			: this(configureLogging,
				() => HttpContext.Current.Server.GetLastError(), 
				new Log4NetLogger(LogManager.GetLogger(typeof(Log4NetModule))))
		{
		}

		public Log4NetModule(Action configureLogging, Func<Exception> getServerError, Log4NetLogger log4NetLogger)
		{
			_configureLogging = configureLogging;
			_getServerError = getServerError;
			_log4NetLogger = log4NetLogger;
		}


		public void Init(HttpApplication application)
		{
			if (!_applicationStarted)
			{
				lock (applicationStartLock)
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
		
		public void OnStart(HttpApplication application)
		{
			_configureLogging.Invoke();
		}
		
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
		
		public void OnInit(HttpApplication application)
		{
			application.Error += Application_Error;
		}

		public void Application_Error(object sender, EventArgs e) { _log4NetLogger.LogException(_getServerError.Invoke()); }
		
		public void Dispose()
		{
		}
	}
}