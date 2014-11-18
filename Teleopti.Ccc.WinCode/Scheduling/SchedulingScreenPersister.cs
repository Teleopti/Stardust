using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class SchedulingScreenPersister : ISchedulingScreenPersister
	{
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;
		private readonly IPersonAccountPersister _personAccountPersister;
		private readonly IRequestPersister _requestPersister;
		private readonly IWriteProtectionPersister _writeProtectionPersister;
		private readonly IWorkflowControlSetPersister _workflowControlSetPersister;

		public SchedulingScreenPersister(IScheduleDictionaryPersister scheduleDictionaryPersister,
																		IPersonAccountPersister personAccountPersister,
																		IRequestPersister requestPersister,
																		IWriteProtectionPersister writeProtectionPersister,
																		IWorkflowControlSetPersister workflowControlSetPersister)
		{
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
			_personAccountPersister = personAccountPersister;
			_requestPersister = requestPersister;
			_writeProtectionPersister = writeProtectionPersister;
			_workflowControlSetPersister = workflowControlSetPersister;
		}

		public bool TryPersist(IScheduleDictionary scheduleDictionary,
														ICollection<IPersonAbsenceAccount> personAbsenceAccounts,
														IEnumerable<IPersonRequest> personRequests,
														ICollection<IPersonWriteProtectionInfo> writeProtectionInfos,
														ICollection<IWorkflowControlSet> workflowControlSets,
														out IEnumerable<PersistConflict> foundConflicts)
		{
			foundConflicts = _scheduleDictionaryPersister.Persist(scheduleDictionary);
			_personAccountPersister.Persist(personAbsenceAccounts);
			_requestPersister.Persist(personRequests);
			_writeProtectionPersister.Persist(writeProtectionInfos);
			_workflowControlSetPersister.Persist(workflowControlSets);
			
			return foundConflicts==null || !foundConflicts.Any();
		}
	}
}