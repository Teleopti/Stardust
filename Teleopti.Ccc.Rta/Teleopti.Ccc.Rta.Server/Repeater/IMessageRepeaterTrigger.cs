using System;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public interface IMessageRepeaterTrigger
	{
		void SetTriggerMethod(Action flush);
	}
}