using System;
using System.Web.Mvc;
using log4net;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	[CLSCompliant(false)]
	public class Log4NetMvCLogger : HandleErrorAttribute
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Log4NetMvCLogger));

		public override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);

			logger.Error(filterContext.Exception.Message, filterContext.Exception);
		}
	}
}