using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentMessageSenders : ICurrentMessageSenders
	{
		private readonly IEnumerable<IMessageSender> _messageSenders;

		public FakeCurrentMessageSenders(IEnumerable<IMessageSender> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<IMessageSender> Current()
		{
			return _messageSenders;
		}
	}
}