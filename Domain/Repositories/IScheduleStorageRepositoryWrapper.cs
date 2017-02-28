using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleStorageRepositoryWrapper
	{
		void Add(IPersistableScheduleData item);
		void Remove(IPersistableScheduleData item);
		IPersistableScheduleData LoadScheduleDataAggregate(Type scheduleDataType, Guid id);
		IPersistableScheduleData Get(Type scheduleDataType, Guid id);


	}
}