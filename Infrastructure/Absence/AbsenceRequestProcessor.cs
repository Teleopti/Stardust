using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequest;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestProcessor : IAbsenceRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestHandler));
		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private readonly IUpdateScheduleProjectionReadModel _updateScheduleProjectionReadModel;
		private readonly ISchedulingResultStateHolderProvider _schedulingResultStateHolderProvider;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AbsenceRequestProcessor(IAbsenceRequestUpdater absenceRequestUpdater,
			IUpdateScheduleProjectionReadModel updateScheduleProjectionReadModel,
			ISchedulingResultStateHolderProvider schedulingResultStateHolderProvider)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_updateScheduleProjectionReadModel = updateScheduleProjectionReadModel;
			_schedulingResultStateHolderProvider = schedulingResultStateHolderProvider;
		}

		public void ProcessAbsenceRequest(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			_schedulingResultStateHolder = _schedulingResultStateHolderProvider.GiveMeANew();

			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, absenceRequest, unitOfWork, _schedulingResultStateHolder))
			{
				return;
			}

			try
			{
				unitOfWork.PersistAll();
			}
			catch (OptimisticLockException ex)
			{
				logger.Error("A optimistic locking error occurred. Review the error log. Processing cannot continue this time.", ex);
				return;
			}
	
			updateScheduleReadModelsIfRequestWasApproved(unitOfWork, absenceRequest, personRequest);
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
	}
}