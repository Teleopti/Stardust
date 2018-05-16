using System.Diagnostics;

namespace Teleopti.Ccc.Web.MyTimeTrafficSimulator
{
	public class Stats
	{
		public int NumberOfCallbacks;
		public Stopwatch Stopwatch = new Stopwatch();
		private readonly object thisLock = new object();

		public void Callback()
		{
			lock (thisLock)
			{
				NumberOfCallbacks++;
			}
		}
	}
}