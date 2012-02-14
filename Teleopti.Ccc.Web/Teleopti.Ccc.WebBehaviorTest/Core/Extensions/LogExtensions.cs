using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Extensions
{

	// was just made to make refact frmo Console.Write* to Log.Warn easier. And now also isolates the logging level decision..
	public static class LogExtensions
	{
		public static void Write(this ILog log, string message)
		{
			log.Warn(message);
		}

		public static void Write(this ILog log, string message, params object[] args)
		{
			log.WarnFormat(message, args);
		}
	}
}