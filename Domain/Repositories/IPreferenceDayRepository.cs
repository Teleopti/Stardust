using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPreferenceDayRepository : IRepository<IPreferenceDay>, ILoadAggregateById<IPreferenceDay>, ILoadAggregateFromBroker<IPreferenceDay>
	{
		IList<IPreferenceDay> Find(DateOnlyPeriod period, IEnumerable<IPerson> persons);
		IList<IPreferenceDay> Find(DateOnly dateOnly, IPerson person);
		IList<IPreferenceDay> FindAndLock(DateOnly dateOnly, IPerson person);
		IList<IPreferenceDay> Find(DateOnlyPeriod period, IPerson person);
		IList<IPreferenceDay> FindNewerThan(DateTime newerThan);
		IPreferenceDay Find(Guid preferenceId);
	}
}