using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
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
		private readonly ISchedulingResultStateHolderProvider _schedulingResultStateHolderProvider;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		public AbsenceRequestProcessor(IAbsenceRequestUpdater absenceRequestUpdater,
			ISchedulingResultStateHolderProvider schedulingResultStateHolderProvider)
		{
			_absenceRequestUpdater = absenceRequestUpdater;
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
			}
		}
	}
}