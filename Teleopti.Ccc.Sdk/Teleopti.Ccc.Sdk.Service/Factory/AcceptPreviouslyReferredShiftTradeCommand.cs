using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class AcceptPreviouslyReferredShiftTradeCommand : IExecutableCommand
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;
		private readonly PersonRequestDto _personRequestDto;

		public AcceptPreviouslyReferredShiftTradeCommand(IScheduleStorage scheduleStorage, IPersonRequestRepository personRequestRepository, ICurrentScenario currentScenario, IMessagePopulatingServiceBusSender serviceBusSender, PersonRequestDto personRequestDto)
		{
			_scheduleStorage = scheduleStorage;
			_personRequestRepository = personRequestRepository;
			_currentScenario = currentScenario;
			_serviceBusSender = serviceBusSender;
			_personRequestDto = personRequestDto;
		}

		public void Execute()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var shiftTradeRequestSetChecksum = new ShiftTradeRequestSetChecksum(_currentScenario, _scheduleStorage);

				var domainPersonRequest = _personRequestRepository.Load(_personRequestDto.Id.GetValueOrDefault(Guid.Empty));
				try
				{
					domainPersonRequest.Request.Accept(domainPersonRequest.Person, shiftTradeRequestSetChecksum, new SdkPersonRequestAuthorizationCheck());
					domainPersonRequest.TrySetMessage(_personRequestDto.Message);
				}
				catch (ShiftTradeRequestStatusException exception)
				{
					throw new FaultException(
						new FaultReason(
							new FaultReasonText(string.Format(CultureInfo.InvariantCulture,
																"The accept action failed. The error was: {0}.",
																exception.Message),
												CultureInfo.InvariantCulture)));
				}
				uow.PersistAll();
			}

			var message = new NewShiftTradeRequestCreated
				{
					PersonRequestId =
						_personRequestDto.Id.GetValueOrDefault(Guid.Empty)
				};
			_serviceBusSender.Send(message, true);
		}
	}
}