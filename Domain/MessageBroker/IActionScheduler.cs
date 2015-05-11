using System;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public interface IActionScheduler
	{
		void Do(Action action);
	}
}