using log4net;

namespace Teleopti.Ccc.Domain.Aop
{
	public class LogManagerWrapper : ILogManagerWrapper
	{
		public ILog GetLogger(string loggerName)
		{
			return LogManager.GetLogger(loggerName);
		}
	}
}