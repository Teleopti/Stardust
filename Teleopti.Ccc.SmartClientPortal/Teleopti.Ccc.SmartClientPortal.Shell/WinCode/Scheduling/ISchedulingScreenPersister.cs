using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
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