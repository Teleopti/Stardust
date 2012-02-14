using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleScreenPersister
	{
		IScheduleScreenPersisterResult TryPersist(IScheduleDictionary scheduleDictionary,
												  ICollection<IPersonWriteProtectionInfo> persons,
												  IEnumerable<IPersonRequest> personRequests,
												  ICollection<IPersonAbsenceAccount> personAbsenceAccounts);
	}
}