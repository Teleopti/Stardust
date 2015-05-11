using System;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class ActionImmediate : IActionScheduler
	{
		public void Do(Action action)
		{
			action.Invoke();
		}
	}
}