using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class DisabledFilterCounter
	{
		private readonly IDictionary<IQueryFilter, int> _disabledFilters;

		public DisabledFilterCounter()
		{
			_disabledFilters = new Dictionary<IQueryFilter, int>();
		}

		public void Increase(IQueryFilter filter)
		{
			createEntryIfNecessary(filter);
			_disabledFilters[filter]++;
		}

		public bool DecreaseAndCheckIfDisabled(IQueryFilter filter)
		{
			createEntryIfNecessary(filter);
			_disabledFilters[filter]--;
			return _disabledFilters[filter] < 1;
		}

		private void createEntryIfNecessary(IQueryFilter filter)
		{
			if(!_disabledFilters.ContainsKey(filter))
				_disabledFilters[filter] = 0;
		}
	}
}