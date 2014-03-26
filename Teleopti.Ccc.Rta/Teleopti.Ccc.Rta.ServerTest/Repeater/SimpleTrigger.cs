using System;
using Teleopti.Ccc.Rta.Server.Repeater;

namespace Teleopti.Ccc.Rta.ServerTest.Repeater
{
	public class SimpleTrigger : IMessageRepeaterTrigger
	{
		private Action _action;

		public void Initialize(Action action)
		{
			_action = action;
		}

		public void TriggerAction()
		{
			_action();
		}
	}
}