using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class DenyLongQueuedAbsenceRequests
	{
		private readonly IQueuedAbsenceRequestRepository _absenceRequestRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public DenyLongQueuedAbsenceRequests(IQueuedAbsenceRequestRepository absenceRequestRepository, ICommandDispatcher commandDispatcher, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_absenceRequestRepository = absenceRequestRepository;
			_commandDispatcher = commandDispatcher;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void DenyAndRemoveLongRunningRequests(IEnumerable<IQueuedAbsenceRequest> longRequests )
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork())
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
				uow.PersistAll();
			}
			
		}
	}
}