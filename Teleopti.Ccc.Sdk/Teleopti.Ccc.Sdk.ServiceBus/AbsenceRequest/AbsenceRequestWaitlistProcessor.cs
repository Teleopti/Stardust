using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest
{
	public class AbsenceRequestWaitlistProcessor : IAbsenceRequestWaitlistProcessor
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(AbsenceRequestWaitlistProcessor));

		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
		private readonly IAbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;

		public AbsenceRequestWaitlistProcessor(IAbsenceRequestUpdater absenceRequestUpdater, ISchedulingResultStateHolder schedulingResultStateHolder, IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel, IAbsenceRequestWaitlistProvider absenceRequestWaitlistProvider)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
			_absenceRequestWaitlistProvider = absenceRequestWaitlistProvider;
		}

		public void ProcessAbsenceRequestWaitlist(IUnitOfWork unitOfWork, DateTimePeriod period, IWorkflowControlSet workflowControlSet)
		{
			var waitlistedRequestsForPeriod = _absenceRequestWaitlistProvider.GetWaitlistedRequests (period, workflowControlSet);
			processRequests(unitOfWork, waitlistedRequestsForPeriod);
		}

		private void processRequests(IUnitOfWork unitOfWork, IEnumerable<IPersonRequest> waitlistedRequests)
		{
			foreach (var request in waitlistedRequests)
			{
				processRequest(unitOfWork, request);

				try
				{
					unitOfWork.PersistAll();
				}
				catch (OptimisticLockException ex)
				{
					logger.Error("A optimistic locking error occurred. Review the error log. Processing cannot continue this time.", ex);
				}
			}

			clearStateHolder(); 
			
		}

		private void processRequest(IUnitOfWork unitOfWork, IPersonRequest request)
		{

			var absenceRequest = request.Request as IAbsenceRequest;
			if (_absenceRequestUpdater.UpdateAbsenceRequest(request, absenceRequest, unitOfWork, _schedulingResultStateHolder))
			{
				if (request.IsApproved)
				{
					updateScheduleReadModels(absenceRequest, request.Person);
				}
			}
		}

		private void updateScheduleReadModels(IRequest absenceRequest, IPerson person)
		{
			var agentTimeZone = person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

			_updateScheduleProjectionReadModel.Execute(_schedulingResultStateHolder.Schedules[person], dateOnlyPeriod);
		}

		private void clearStateHolder()
		{
			_schedulingResultStateHolder.Dispose();
			_schedulingResultStateHolder = null;
		}

	}
}
