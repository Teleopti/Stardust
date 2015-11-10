using System;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core.Logging
{
	[CLSCompliant(false)]
	public class Log4NetMvCLogger : HandleErrorAttribute
	{
		private readonly Log4NetLogger _log4NetLogger;
		private readonly bool customErrorsIsDisabledAndSomethingElseWillWriteToTheEventLog;

		public Log4NetMvCLogger(Log4NetLogger log4NetLogger)
		{
			Order = 0;
			_log4NetLogger = log4NetLogger;
			var configuration = WebConfigurationManager.OpenWebConfiguration("/");
			var customErrorsSection = (CustomErrorsSection)configuration.GetSection("system.web/customErrors");
			customErrorsIsDisabledAndSomethingElseWillWriteToTheEventLog = customErrorsSection.Mode == CustomErrorsMode.Off;
		}

		public override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);
			if (customErrorsIsDisabledAndSomethingElseWillWriteToTheEventLog) return;
			
			_log4NetLogger.LogException(filterContext.Exception);
		}
	}
}