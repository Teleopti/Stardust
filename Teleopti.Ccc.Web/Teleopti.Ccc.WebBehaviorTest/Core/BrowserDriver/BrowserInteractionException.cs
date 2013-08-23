using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver
{
	[Serializable]
	public class BrowserInteractionException : Exception
	{
		public BrowserInteractionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}