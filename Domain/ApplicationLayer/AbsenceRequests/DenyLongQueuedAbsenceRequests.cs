using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class DenyLongQueuedAbsenceRequests
	{
		private readonly IQueuedAbsenceRequestRepository _absenceRequestRepository;
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IConfigReader _configReader;

		public DenyLongQueuedAbsenceRequests(IQueuedAbsenceRequestRepository absenceRequestRepository, ICommandDispatcher commandDispatcher, IConfigReader configReader)
		{
			_absenceRequestRepository = absenceRequestRepository;
			_commandDispatcher = commandDispatcher;
			_configReader = configReader;
		} 

		public IList<IQueuedAbsenceRequest> DenyAndRemoveLongRunningRequests(IEnumerable<IQueuedAbsenceRequest> allRequests)
		{
			var maxDaysForAbsenceRequest = _configReader.ReadValue("MaximumDayLengthForAbsenceRequest", 40);
			var longRequests = allRequests.Where(x => x.EndDateTime.Subtract(x.StartDateTime).TotalDays >= maxDaysForAbsenceRequest).ToList();
			
			longRequests.ForEach(request =>
			{
				if (request.PersonRequest != Guid.Empty)
				{
					var command = new DenyRequestCommand
					{
						PersonRequestId = request.PersonRequest,
						DenyReason = UserTexts.Resources.RequestedPeriodIsTooLong,
						DenyOption = PersonRequestDenyOption.PeriodToLong
					};
					_commandDispatcher.Execute(command);
				}
				_absenceRequestRepository.Remove(request);
			});

			return allRequests.Except(longRequests).ToList();
		}
	}
}