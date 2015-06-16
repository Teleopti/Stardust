using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface ICurrentMessageSenders
	{
		IEnumerable<IMessageSender> Current();
	}
}