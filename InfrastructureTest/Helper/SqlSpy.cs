using System;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
	public class SqlSpy : IDisposable
	{
		private readonly MemoryAppender _appender;
		private readonly Logger _logger;
		private readonly Level _prevLogLevel;

		public SqlSpy()
		{
			_logger = (Logger)LogManager.GetLogger(GetType().Assembly, "NHibernate.SQL").Logger;

			// Change the log level to DEBUG and temporarily save the previous log level
			_prevLogLevel = _logger.Level;
			_logger.Level = Level.Debug;
			
			// Add a new MemoryAppender to the logger.
			_appender = new MemoryAppender();
			_logger.AddAppender(_appender);
		}

		public string WholeLog()
		{
			var wholeMessage = new StringBuilder();
			foreach (var loggingEvent in _appender.GetEvents())
			{
				wholeMessage
					.Append(loggingEvent.LoggerName)
					.Append(" ")
					.Append(loggingEvent.RenderedMessage)
					.AppendLine();
			}
			return wholeMessage.ToString();
		}

		public void Dispose()
		{
			_logger.Level = _prevLogLevel;
			_logger.RemoveAppender(_appender);
		}
	}
}