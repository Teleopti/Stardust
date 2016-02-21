using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest
{
	public class AbsenceRequestProcessor : IAbsenceRequestProcessor
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestConsumer));
		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AbsenceRequestProcessor(IAbsenceRequestUpdater absenceRequestUpdater, IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public void ProcessAbsenceRequest(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, absenceRequest, unitOfWork, _schedulingResultStateHolder))
			{
				clearStateHolder();
				return;
			}

			try
			{
				unitOfWork.PersistAll();
			}
			catch (OptimisticLockException ex)
			{
				logger.Error("A optimistic locking error occurred. Review the error log. Processing cannot continue this time.", ex);
				clearStateHolder();
				return;
			}

			updateScheduleReadModelsIfRequestWasApproved(unitOfWork, absenceRequest, personRequest);

			clearStateHolder();
		}
	
		private void updateScheduleReadModelsIfRequestWasApproved(IUnitOfWork unitOfWork, IRequest absenceRequest, IPersonRequest personRequest)
		{
			var agentTimeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = absenceRequest.Period.ToDateOnlyPeriod(agentTimeZone);

			if (personRequest.IsApproved)
			{
				_updateScheduleProjectionReadModel.Execute(_schedulingResultStateHolder.Schedules[absenceRequest.Person], dateOnlyPeriod);

				unitOfWork.PersistAll();
			}
		}

		private void clearStateHolder()
		{
			_schedulingResultStateHolder.Dispose();
			_schedulingResultStateHolder = null;
		}
		
	}
}