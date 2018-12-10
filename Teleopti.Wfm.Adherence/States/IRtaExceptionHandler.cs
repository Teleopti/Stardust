using System;
using log4net;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IRtaExceptionHandler
	{
		bool InvalidAuthenticationKey(InvalidAuthenticationKeyException exception);
		bool LegacyAuthenticationKey(LegacyAuthenticationKeyException exception);
		bool InvalidSource(InvalidSourceException exception);
		bool InvalidUserCode(InvalidUserCodeException exception);
		bool OtherException(Exception exception);
	}

	public class ThrowAll : IRtaExceptionHandler
	{
		public bool InvalidAuthenticationKey(InvalidAuthenticationKeyException exception)
		{
			return false;
		}

		public bool LegacyAuthenticationKey(LegacyAuthenticationKeyException exception)
		{
			return false;
		}

		public bool InvalidSource(InvalidSourceException exception)
		{
			return false;
		}

		public bool InvalidUserCode(InvalidUserCodeException exception)
		{
			return false;
		}

		public bool OtherException(Exception exception)
		{
			return false;
		}
	}

	public class CatchAndLogAll : IRtaExceptionHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CatchAndLogAll));

		public bool InvalidAuthenticationKey(InvalidAuthenticationKeyException exception)
		{
			Log.Error("Unhandled exception occurred processing states from queue", exception);
			return true;
		}

		public bool LegacyAuthenticationKey(LegacyAuthenticationKeyException exception)
		{
			Log.Error("Unhandled exception occurred processing states from queue", exception);
			return true;
		}

		public bool InvalidSource(InvalidSourceException exception)
		{
			Log.Error("Source id was invalid.", exception);
			return true;
		}

		public bool InvalidUserCode(InvalidUserCodeException exception)
		{
			Log.Info("User code was invalid.", exception);
			return true;
		}

		public bool OtherException(Exception exception)
		{
			Log.Error("Unhandled exception occurred processing states from queue", exception);
			return true;
		}
	}

	public class InvalidInputMessage : IRtaExceptionHandler
	{
		public string Message;

		public bool InvalidAuthenticationKey(InvalidAuthenticationKeyException exception)
		{
			Message = exception.Message;
			return true;
		}

		public bool LegacyAuthenticationKey(LegacyAuthenticationKeyException exception)
		{
			Message = exception.Message;
			return true;
		}

		public bool InvalidSource(InvalidSourceException exception)
		{
			Message = exception.Message;
			return true;
		}

		public bool InvalidUserCode(InvalidUserCodeException exception)
		{
			Message = exception.Message;
			return true;
		}

		public bool OtherException(Exception exception)
		{
			return false;
		}
	}

	public class LegacyReturnValue : IRtaExceptionHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(LegacyReturnValue));

		public int ReturnValue = 1;

		public bool InvalidAuthenticationKey(InvalidAuthenticationKeyException exception)
		{
			return false;
		}

		public bool LegacyAuthenticationKey(LegacyAuthenticationKeyException exception)
		{
			return false;
		}

		public bool InvalidSource(InvalidSourceException exception)
		{
			Log.Error("Source id was invalid.", exception);
			ReturnValue = -300;
			return true;
		}

		public bool InvalidUserCode(InvalidUserCodeException exception)
		{
			Log.Info("User code was invalid.", exception);
			ReturnValue = -100;
			return true;
		}

		public bool OtherException(Exception exception)
		{
			return false;
		}
	}
}