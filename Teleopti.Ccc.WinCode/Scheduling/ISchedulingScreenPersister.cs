using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface ISchedulingScreenPersister
	{
		bool TryPersist(IScheduleDictionary scheduleDictionary,
		                ICollection<IPersonAbsenceAccount> personAbsenceAccounts,
		                IEnumerable<IPersonRequest> personRequests,
		                ICollection<IPersonWriteProtectionInfo> writeProtectionInfos,
						ICollection<IWorkflowControlSet> workflowControlSets,
						out IEnumerable<PersistConflict> foundConflicts);
	}
}