using log4net;

namespace Teleopti.Ccc.Domain.Aop
{
	public class TestLog
	{
		private readonly ILog _log;

		public TestLog(ILogManager logManager)
		{
			_log = logManager.GetLogger("Teleopti.TestLog");
		}

		public bool IsEnabled()
		{
			return _log.IsDebugEnabled;
		}

		public void Debug(string message)
		{
			_log.Debug(message);
		}

		public static class Static
		{
			public static void Debug(string message)
			{
				LogManager.GetLogger("Teleopti.TestLog").Debug(message);
			}
		}
	}
}