using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class DenyLongQueuedAbsenceRequests
	{
		private readonly IQueuedAbsenceRequestRepository _absenceRequestRepository;
		private readonly ICommandDispatcher _commandDispatcher;

		public DenyLongQueuedAbsenceRequests(IQueuedAbsenceRequestRepository absenceRequestRepository, ICommandDispatcher commandDispatcher)
		{
			_absenceRequestRepository = absenceRequestRepository;
			_commandDispatcher = commandDispatcher;
		}

		public void DenyAndRemoveLongRunningRequests(IEnumerable<IQueuedAbsenceRequest> longRequests )
		{
			longRequests.ForEach(request =>
			{
				var command = new DenyRequestCommand()
				{
					PersonRequestId = request.PersonRequest,
					DenyReason = UserTexts.Resources.RequestedPeriodIsTooLong,
					DenyOption = PersonRequestDenyOption.None
				};
				_commandDispatcher.Execute(command);
				_absenceRequestRepository.Remove(request);
			});
			
		}
	}
}