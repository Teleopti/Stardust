using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class NoMessageSenders : ICurrentMessageSenders
	{
		public IEnumerable<IMessageSender> Current()
		{
			yield break;
		}
	}
}