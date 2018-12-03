using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.Infrastructure.Persisters.Requests;
using Teleopti.Ccc.Infrastructure.Persisters.WriteProtection;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class SchedulingScreenPersister : ISchedulingScreenPersister
	{
		private readonly IScheduleDictionaryPersister _scheduleDictionaryPersister;
		private readonly IRequestPersister _requestPersister;
		private readonly IWriteProtectionPersister _writeProtectionPersister;
		private readonly IWorkflowControlSetPublishDatePersister _workflowControlSetPublishDatePersister;

		public SchedulingScreenPersister(
			IScheduleDictionaryPersister scheduleDictionaryPersister,
			IRequestPersister requestPersister,
			IWriteProtectionPersister writeProtectionPersister,
			IWorkflowControlSetPublishDatePersister workflowControlSetPublishDatePersister)
		{
			_scheduleDictionaryPersister = scheduleDictionaryPersister;
			_requestPersister = requestPersister;
			_writeProtectionPersister = writeProtectionPersister;
			_workflowControlSetPublishDatePersister = workflowControlSetPublishDatePersister;
		}

		public bool TryPersist(IScheduleDictionary scheduleDictionary,
			IEnumerable<IPersonRequest> personRequests,
			ICollection<IPersonWriteProtectionInfo> writeProtectionInfos,
			ICollection<IWorkflowControlSet> workflowControlSets,
			out IEnumerable<PersistConflict> foundConflicts)
		{
			foundConflicts = _scheduleDictionaryPersister.Persist(scheduleDictionary);
			_requestPersister.Persist(personRequests);
			_writeProtectionPersister.Persist(writeProtectionInfos);
			_workflowControlSetPublishDatePersister.Persist(workflowControlSets);

			return foundConflicts == null || !foundConflicts.Any();
		}
	}
}