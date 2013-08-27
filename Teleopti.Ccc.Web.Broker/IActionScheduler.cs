using System;

namespace Teleopti.Ccc.Web.Broker
{
	public interface IActionScheduler
	{
		void Do(Action action);
	}
}