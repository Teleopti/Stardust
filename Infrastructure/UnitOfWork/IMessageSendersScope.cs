using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IMessageSendersScope
	{
		IDisposable GloballyUse(IEnumerable<IMessageSender> messageSenders);
	}
}