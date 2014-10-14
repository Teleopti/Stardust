using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public class NotExecutableScriptException : ArgumentException
	{
		public NotExecutableScriptException()
		{
		}

		public NotExecutableScriptException(string fileName, string paramName, Exception innerException)
			: base(
				"File name: " + fileName + Environment.NewLine + Environment.NewLine + innerException.Message, paramName,
				innerException)
		{
		}
	}
}