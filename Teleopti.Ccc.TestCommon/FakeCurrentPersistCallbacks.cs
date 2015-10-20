using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentPersistCallbacks : ICurrentPersistCallbacks
	{
		private readonly IEnumerable<IPersistCallback> _messageSenders;

		public FakeCurrentPersistCallbacks(IEnumerable<IPersistCallback> messageSenders)
		{
			_messageSenders = messageSenders;
		}

		public IEnumerable<IPersistCallback> Current()
		{
			return _messageSenders;
		}
	}
}