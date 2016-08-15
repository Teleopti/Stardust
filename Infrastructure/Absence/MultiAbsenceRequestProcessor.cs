using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class MultiAbsenceRequestProcessor : IMultiAbsenceRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(MultiAbsenceRequestProcessor));
		private readonly MultiAbsenceRequestsUpdater _absenceRequestUpdater;
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;

		public MultiAbsenceRequestProcessor(MultiAbsenceRequestsUpdater absenceRequestUpdater,
			Func<ISchedulingResultStateHolder> scheduleResultStateHolder)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_scheduleResultStateHolder = scheduleResultStateHolder;
		}

		public void ProcessAbsenceRequest(IUnitOfWork unitOfWork, List<IPersonRequest> personRequests)
		{
			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequests, unitOfWork, _scheduleResultStateHolder(), null, null))
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
			}
		}

		public void ApproveAbsenceRequestWithValidators(List<IPersonRequest> personRequest, IUnitOfWork unitOfWork, IEnumerable<IAbsenceRequestValidator> validators)
		{
			var grantAbsenceRequest = new ApproveAbsenceRequestWithValidators();
			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, unitOfWork,
				_scheduleResultStateHolder(), grantAbsenceRequest, validators))
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
			}
		}
	}
}