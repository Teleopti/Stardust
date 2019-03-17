using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli
{
	public class SikuliCreateInstanceException : Exception
	{
		public SikuliCreateInstanceException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}