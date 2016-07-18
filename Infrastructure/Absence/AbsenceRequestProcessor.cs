using System;
using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public void ProcessAbsenceRequest(IUnitOfWork unitOfWork, IAbsenceRequest absenceRequest, IPersonRequest personRequest)
		{
			if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, absenceRequest, unitOfWork, _scheduleResultStateHolder(), null, null))
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

	    public void ApproveAbsenceRequestWithValidators(IPersonRequest personRequest,IAbsenceRequest absenceRequest, IUnitOfWork unitOfWork,IEnumerable<IAbsenceRequestValidator> validators)
	    {
            var grantAbsenceRequest = new ApproveAbsenceRequestWithValidators();

	        if (!_absenceRequestUpdater.UpdateAbsenceRequest(personRequest, absenceRequest, unitOfWork,
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