using System;
using System.Collections.Generic;
using log4net;
using log4net.Core;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class LogSpy : ILog
	{
		public LogSpy()
		{
			DebugMessages = new List<string>();
			ErrorMessages = new List<string>();
		}

		public ILogger Logger { get; private set; }
		public string InfoMessage { get; private set; }
		public IList<string> DebugMessages { get; private set; }
		public IList<string> ErrorMessages { get; private set; }
		public string LastLoggerName { get; set; }

		public void Debug(object message)
		{
			DebugMessages.Add(message.ToString());
		}

		public void Debug(object message, Exception exception)
		{
			DebugMessages.Add(message.ToString());
		}

		public void DebugFormat(string format, params object[] args)
		{
			DebugMessages.Add(string.Format(format, args));
		}

		public void DebugFormat(string format, object arg0)
		{
			DebugMessages.Add(string.Format(format, arg0));
		}

		public void DebugFormat(string format, object arg0, object arg1)
		{
			DebugMessages.Add(string.Format(format, arg0, arg1));
		}

		public void DebugFormat(string format, object arg0, object arg1, object arg2)
		{
			DebugMessages.Add(string.Format(format, arg0, arg1, arg2));
		}

		public void DebugFormat(IFormatProvider provider, string format, params object[] args)
		{
			DebugMessages.Add(string.Format(format, args));
		}

		public void Info(object message)
		{
			InfoMessage += message;
		}

		public void Info(object message, Exception exception)
		{
			InfoMessage += message;
		}

		public void InfoFormat(string format, params object[] args)
		{
			InfoMessage += string.Format(format, args);
		}

		public void InfoFormat(string format, object arg0)
		{
			InfoMessage += string.Format(format, arg0);
		}

		public void InfoFormat(string format, object arg0, object arg1)
		{
			InfoMessage += string.Format(format, arg0, arg1);
		}

		public void InfoFormat(string format, object arg0, object arg1, object arg2)
		{
			InfoMessage += string.Format(format, arg0, arg1, arg2);
		}

		public void InfoFormat(IFormatProvider provider, string format, params object[] args)
		{
			InfoMessage += string.Format(provider, format, args);
		}

		public void Warn(object message)
		{
		}

		public void Warn(object message, Exception exception)
		{
		}

		public void WarnFormat(string format, params object[] args)
		{
		}

		public void WarnFormat(string format, object arg0)
		{
		}

		public void WarnFormat(string format, object arg0, object arg1)
		{
		}

		public void WarnFormat(string format, object arg0, object arg1, object arg2)
		{
		}

		public void WarnFormat(IFormatProvider provider, string format, params object[] args)
		{
		}

		public void Error(object message)
		{
			ErrorMessages.Add(message.ToString());
		}

		public void Error(object message, Exception exception)
		{
			ErrorMessages.Add(message.ToString());
		}

		public void ErrorFormat(string format, params object[] args)
		{
			ErrorMessages.Add(string.Format(format, args));
		}

		public void ErrorFormat(string format, object arg0)
		{
			ErrorMessages.Add(string.Format(format, arg0));
		}

		public void ErrorFormat(string format, object arg0, object arg1)
		{
			ErrorMessages.Add(string.Format(format, arg0, arg1));
		}

		public void ErrorFormat(string format, object arg0, object arg1, object arg2)
		{
			ErrorMessages.Add(string.Format(format, arg0, arg1, arg2));
		}

		public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
		{
			ErrorMessages.Add(string.Format(provider, format, args));
		}

		public void Fatal(object message)
		{
		}

		public void Fatal(object message, Exception exception)
		{
		}

		public void FatalFormat(string format, params object[] args)
		{
		}

		public void FatalFormat(string format, object arg0)
		{
		}

		public void FatalFormat(string format, object arg0, object arg1)
		{
		}

		public void FatalFormat(string format, object arg0, object arg1, object arg2)
		{
		}

		public void FatalFormat(IFormatProvider provider, string format, params object[] args)
		{
		}

		public bool IsDebugEnabled { get; private set; }
		public bool IsInfoEnabled { get { return true; } }
		public bool IsWarnEnabled { get; private set; }
		public bool IsErrorEnabled { get; private set; }
		public bool IsFatalEnabled { get; private set; }
	}
}