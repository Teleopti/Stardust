using System;

namespace Teleopti.Interfaces
{
	/// <summary>
	/// blah
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "WTFDEBUG")]
	public static class WTFDEBUG
	{
		/// <summary>
		/// Blah!
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void Clear()
		{
			try
			{
				System.IO.File.Delete(@"wtfdebug.log");
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.Delete(@"C:\wtfdebug.log");
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.Delete(@"C:\inetpub\wwwroot\PBI20491-AgentPortalWeb\wtfdebug.log");
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.Delete(@"C:\Program Files (x86)\CruiseControl.NET\server\PBI20491\WorkingDirectory\wtfdebug.log");
			}
			catch (Exception)
			{
			}
		}
		/// <summary>
		/// Blah!!
		/// </summary>
		/// <param name="stuff"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public static void Log(string stuff)
		{
			try
			{
				System.IO.File.AppendAllText(@"wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\inetpub\wwwroot\PBI20491-AgentPortalWeb\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
			try
			{
				System.IO.File.AppendAllText(@"C:\Program Files (x86)\CruiseControl.NET\server\PBI20491\WorkingDirectory\wtfdebug.log", stuff + Environment.NewLine);
			}
			catch (Exception)
			{
			}
		}
	}
}