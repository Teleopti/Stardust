using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Absence
{
	public class AbsenceRequestWaitlistProcessor : IAbsenceRequestWaitlistProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AbsenceRequestWaitlistProcessor));
		private readonly IAbsenceRequestUpdater _absenceRequestUpdater;
		private readonly Func<ISchedulingResultStateHolder> _scheduleResultStateHolder;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IAbsenceRequestWaitlistProvider _absenceRequestWaitlistProvider;

		public AbsenceRequestWaitlistProcessor(IAbsenceRequestUpdater absenceRequestUpdater,
			Func<ISchedulingResultStateHolder> scheduleResultStateHolder,
			IAbsenceRequestWaitlistProvider absenceRequestWaitlistProvider)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
			_scheduleResultStateHolder = scheduleResultStateHolder;
			_absenceRequestWaitlistProvider = absenceRequestWaitlistProvider;
		}

		public void ProcessAbsenceRequestWaitlist(IUnitOfWork unitOfWork, DateTimePeriod period,
			IWorkflowControlSet workflowControlSet)
		{
			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Process absence request waitlist from \"{period.StartDateTime:yyyy-MM-dd HH:mm:ss}\" "
							 + $"to \"{period.EndDateTime:yyyy-MM-dd HH:mm:ss}\" with workflow controlset "
							 + "\"{workflowControlSet.Name}\"");
			}
			_schedulingResultStateHolder = _scheduleResultStateHolder();
			var waitlistedRequestsForPeriod =
				_absenceRequestWaitlistProvider.GetWaitlistedRequests(period, workflowControlSet).ToList();
			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Total {waitlistedRequestsForPeriod.Count} loaded.");
			}

			processRequests(unitOfWork, waitlistedRequestsForPeriod);
			_schedulingResultStateHolder.ClearAbsenceDataDuringCurrentRequestHandlingCycle();
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
					logger.Error("A optimistic locking error occurred. Review the error log. "
						+"Processing cannot continue this time.", ex);
				}
			}
		}

		private void processRequest(IUnitOfWork unitOfWork, IPersonRequest request)
		{
			if (logger.IsDebugEnabled)
			{
				logger.Debug($"Processing absence request with Id=\"{request.Id}...");
			}
			var absenceRequest = request.Request as IAbsenceRequest;
			_absenceRequestUpdater.UpdateAbsenceRequest(request, absenceRequest, unitOfWork,
				_schedulingResultStateHolder);
		}
	}
}