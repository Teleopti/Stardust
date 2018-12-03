using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonAbsenceRepository : IRepository<IPersonAbsence>, IWriteSideRepository<IPersonAbsence>, ILoadAggregateFromBroker<IPersonAbsence>
	{
		ICollection<IPersonAbsence> Find(IEnumerable<IPerson> persons,
						 DateTimePeriod period,
						 IScenario scenario);
	
		ICollection<IPersonAbsence> Find(DateTimePeriod period, IScenario scenario);
		
		ICollection<IPersonAbsence> Find(IEnumerable<Guid> personAbsenceIds, IScenario scenario);

		ICollection<DateTimePeriod> AffectedPeriods(IPerson person, IScenario scenario, DateTimePeriod period, IAbsence absence = null);
	
		ICollection<IPersonAbsence> FindExact(IPerson person, DateTimePeriod period, IAbsence absence,
			IScenario scenario);

		bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period);
	}
}