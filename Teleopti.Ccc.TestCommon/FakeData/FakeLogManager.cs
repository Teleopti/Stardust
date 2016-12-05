using log4net;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeLogManager : ILogManager
	{
		private readonly LogSpy _logger;

		public FakeLogManager(ILog logger)
		{
			_logger = (LogSpy)logger;
		}

		public ILog GetLogger(string loggerName)
		{
			_logger.LastLoggerName = loggerName;
			return _logger;
		}
	}
}