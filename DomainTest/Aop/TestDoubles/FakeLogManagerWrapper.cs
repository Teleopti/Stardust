using log4net;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.DomainTest.Aop.TestDoubles
{
	public class FakeLogManagerWrapper : ILogManagerWrapper
	{
		private readonly LogSpy _logger;

		public FakeLogManagerWrapper(ILog logger)
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