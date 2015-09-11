using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class NullLog : IUpgradeLog
	{
		public void Write(string message)
		{
			
		}

		public void Write(string message, string level)
		{
			
		}

		public void Dispose()
		{
			
		}
	}
}