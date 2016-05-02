﻿using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestWaitlistProcessor : IAbsenceRequestWaitlistProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AbsenceRequestWaitlistProcessor));

		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private readonly ISchedulingResultStateHolderProvider _schedulingResultStateHolderProvider;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IAbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;

		public AbsenceRequestWaitlistProcessor(IAbsenceRequestUpdater absenceRequestUpdater,
			ISchedulingResultStateHolderProvider schedulingResultStateHolderProvider,
			IAbsenceRequestWaitlistProvider absenceRequestWaitlistProvider)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_schedulingResultStateHolderProvider = schedulingResultStateHolderProvider;
			_absenceRequestWaitlistProvider = absenceRequestWaitlistProvider;
		}

		public void ProcessAbsenceRequestWaitlist(IUnitOfWork unitOfWork, DateTimePeriod period, IWorkflowControlSet workflowControlSet)
		{
			_schedulingResultStateHolder = _schedulingResultStateHolderProvider.GiveMeANew();
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
		}

		private void processRequest(IUnitOfWork unitOfWork, IPersonRequest request)
		{
			var absenceRequest = request.Request as IAbsenceRequest;
			_absenceRequestUpdater.UpdateAbsenceRequest(request, absenceRequest, unitOfWork, _schedulingResultStateHolder);
		}
		
	}
}
