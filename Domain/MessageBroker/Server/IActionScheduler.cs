using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public interface IActionScheduler
	{
		void Do(Action action);
	}
}