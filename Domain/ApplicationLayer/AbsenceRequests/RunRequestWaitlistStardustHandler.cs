using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RunRequestWaitlistStardustHandler : IHandleEvent<RunRequestWaitlistEvent>, IRunOnStardust
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private IAbsenceRequestWaitlistProcessor _processor;
		private readonly IWorkflowControlSetRepository _wcsRepository;

		public RunRequestWaitlistStardustHandler(ICurrentUnitOfWork currentUnitOfWork,
			IAbsenceRequestWaitlistProcessor processor,
			IWorkflowControlSetRepository wcsRepository)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_processor = processor;
			_wcsRepository = wcsRepository;
		}

		public void Handle(RunRequestWaitlistEvent @event)
		{
			var uow = _currentUnitOfWork.Current();
			var workflowControlSets = _wcsRepository.LoadAll();
			foreach (var wcs in workflowControlSets)
			{
				_processor.ProcessAbsenceRequestWaitlist(uow, @event.Period, wcs);
			}
		}
	}
}