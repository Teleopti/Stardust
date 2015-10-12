using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface ISchedulingScreenPersister
	{
		bool TryPersist(IScheduleDictionary scheduleDictionary,
		                IEnumerable<IPersonRequest> personRequests,
		                ICollection<IPersonWriteProtectionInfo> writeProtectionInfos,
						ICollection<IWorkflowControlSet> workflowControlSets,
						out IEnumerable<PersistConflict> foundConflicts);

		void PersistPersonAccounts(ICollection<IPersonAbsenceAccount> personAbsenceAccounts);
	}
}