using System;

namespace Teleopti.Ccc.Web.Broker
{
	public class ActionImmediate : IActionScheduler
	{
		public void Do(Action action)
		{
			action.Invoke();
		}
	}
}