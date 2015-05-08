using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IWorkflowControlSetPublishDatePersister
	{
		void Persist(ICollection<IWorkflowControlSet> workflowControlSets);
	}

	public class WorkflowControlSetPublishDatePersister : IWorkflowControlSetPublishDatePersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly IInitiatorIdentifier _initiatorIdentifier;

		public WorkflowControlSetPublishDatePersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IWorkflowControlSetRepository workflowControlSetRepository, IInitiatorIdentifier initiatorIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_workflowControlSetRepository = workflowControlSetRepository;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void Persist(ICollection<IWorkflowControlSet> workflowControlSets)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				foreach (var workflowControlSet in workflowControlSets)
				{
					var date = workflowControlSet.SchedulePublishedToDate;
					unitOfWork.Refresh(workflowControlSet);
					workflowControlSet.SchedulePublishedToDate = date;
				}

				_workflowControlSetRepository.AddRange(workflowControlSets);
				unitOfWork.PersistAll(_initiatorIdentifier);
				unitOfWork.Reassociate(workflowControlSets);
				workflowControlSets.Clear();
			}
		}
	}
}
