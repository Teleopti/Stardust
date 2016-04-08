using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentTransactionHooks : ICurrentTransactionHooks
	{
		private readonly IEnumerable<ITransactionHook> _messageSenders;

		public FakeCurrentTransactionHooks(IEnumerable<ITransactionHook> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<ITransactionHook> Current()
		{
			return _messageSenders;
		}
	}
}