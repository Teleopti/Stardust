using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class DbManagerLog4Net : IUpgradeLog
	{
		private readonly ILog logger;

		public DbManagerLog4Net(string name)
		{
			logger = LogManager.GetLogger(name);
		}

		public void Write(string message)
		{
			logger.Info(message);
		}

		public void Write(string message, string level)
		{
			if (level == "FATAL")
				logger.Fatal(message);
			else if (level == "ERROR")
				logger.Error(message);
			else if (level == "WARN")
				logger.Warn(message);
			else if (level == "INFO")
				logger.Info(message);
			else if (level == "DEBUG")
				logger.Debug(message);
			else
				logger.Info(message);
		}

		public void Dispose()
		{
		}
	}
}