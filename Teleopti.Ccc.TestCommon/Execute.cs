using System;
using System.Threading;

namespace Teleopti.Ccc.TestCommon
{
	public static class Execute
	{
		public static Thread OnAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}
	}
}