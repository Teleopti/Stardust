using System;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
	public class AcceptPreviouslyReferredShiftTradeCommand : IExecutableCommand
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IEventPublisher _publisher;
		private readonly PersonRequestDto _personRequestDto;

		public AcceptPreviouslyReferredShiftTradeCommand(IScheduleStorage scheduleStorage,
			IPersonRequestRepository personRequestRepository, ICurrentScenario currentScenario,
			IEventPublisher publisher, PersonRequestDto personRequestDto)
		{
			_scheduleStorage = scheduleStorage;
			_personRequestRepository = personRequestRepository;
			_currentScenario = currentScenario;
			_publisher = publisher;
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
					((IShiftTradeRequest)domainPersonRequest.Request).Accept(domainPersonRequest.Person, shiftTradeRequestSetChecksum, new SdkPersonRequestAuthorizationCheck());
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

			var @event = new NewShiftTradeRequestCreatedEvent
			{
				PersonRequestId =
						_personRequestDto.Id.GetValueOrDefault(Guid.Empty)
			};
			_publisher.Publish(@event);
		}
	}
}