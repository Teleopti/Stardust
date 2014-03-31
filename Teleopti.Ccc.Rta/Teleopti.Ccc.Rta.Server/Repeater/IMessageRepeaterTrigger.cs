using System;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public interface IMessageRepeaterTrigger
	{
		void Initialize(Action flush);
	}
}