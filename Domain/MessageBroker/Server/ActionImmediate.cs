using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class ActionImmediate : IActionScheduler
	{
		public void Do(Action action)
		{
			action.Invoke();
		}
	}
}