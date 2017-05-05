using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestProcessor : IAbsenceRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AbsenceRequestProcessor));
		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;

		public AbsenceRequestProcessor(IAbsenceRequestUpdater absenceRequestUpdater,
			Func<ISchedulingResultStateHolder> scheduleResultStateHolder)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_scheduleResultStateHolder = scheduleResultStateHolder;
		}

		public void ApproveAbsenceRequestWithValidators(IPersonRequest personRequest, IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork, IEnumerable<IAbsenceRequestValidator> validators)
		{
			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, absenceRequest, unitOfWork,
				_scheduleResultStateHolder(), validators))
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