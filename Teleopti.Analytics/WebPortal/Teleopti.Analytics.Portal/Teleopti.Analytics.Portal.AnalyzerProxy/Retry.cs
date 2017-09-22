using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace Teleopti.Analytics.Portal.AnalyzerProxy
{
	public static class Retry
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Retry));
		public static void Do(
			Action action,
			TimeSpan retryInterval,
			int retryCount = 3)
		{
			Do<object>(() =>
			{
				action();
				return null;
			}, retryInterval, retryCount);
		}

		public static T Do<T>(
			Func<T> action,
			TimeSpan retryInterval,
			int retryCount = 3)
		{
			var exceptions = new List<Exception>();

			for (int retry = 0; retry < retryCount; retry++)
			{
				try
				{

					if (retry > 0)
					{
						_log.DebugFormat("Retry no {0}: running method {1}.{2}", retry, action.Target.GetType(), action.Method.Name);
						Thread.Sleep(retryInterval);
					}					
					return action();
				}
				catch (Exception ex)
				{
					_log.ErrorFormat("Error in attempt no {0}: running method {1}.{2} error info:{3}", retry, action.Target.GetType(), action.Method.Name, ex.Message);
					exceptions.Add(ex);
				}
			}

			throw new AggregateException(exceptions);
		}
	}
}
