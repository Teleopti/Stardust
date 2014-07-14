using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMappingEngine _mapper;
		private readonly IServiceBusEventPublisher _serviceBusSender;
		private readonly INow _nu;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestSetChecksum;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository,
									IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
									IPersonRequestCheckAuthorization personRequestCheckAuthorization,
									ILoggedOnUser loggedOnUser,
									IMappingEngine mapper,
									IServiceBusEventPublisher serviceBusSender,
									INow nu)
		{
			_personRequestRepository = personRequestRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_loggedOnUser = loggedOnUser;
			_mapper = mapper;
			_serviceBusSender = serviceBusSender;
			_nu = nu;
			_shiftTradeRequestSetChecksum = shiftTradeRequestSetChecksum;
		}

		public RequestViewModel OkByMe(Guid requestId, string message)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}
			personRequest.TrySetMessage(message);
			persistWithBus(personRequest);

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		public RequestViewModel Deny(Guid requestId)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}
			personRequest.TrySetMessage(personRequest.GetMessage(new NoFormatting()));
			personRequest.Deny(_loggedOnUser.CurrentUser(), "RequestDenyReasonOtherPart", _personRequestCheckAuthorization);

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		public RequestViewModel ResendReferred(Guid requestId)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}

			personRequest.Request.Accept(personRequest.Person, _shiftTradeRequestSetChecksum, _personRequestCheckAuthorization);
			if (_serviceBusSender.EnsureBus())
			{
				_serviceBusSender.Publish(new NewShiftTradeRequestCreated
				{
					PersonRequestId = personRequest.Id.GetValueOrDefault()
				});
			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		private void persistWithBus(IPersonRequest personRequest)
		{
			if (_serviceBusSender.EnsureBus())
			{
				_serviceBusSender.Publish(new AcceptShiftTrade
													   {
														   PersonRequestId = personRequest.Id.GetValueOrDefault(),
														   AcceptingPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
														   Message = personRequest.GetMessage(new NoFormatting())
													   });
			}
			else
			{
				var shittrade = (IShiftTradeRequest)personRequest.Request;
				shittrade.Accept(_loggedOnUser.CurrentUser(), _shiftTradeRequestSetChecksum, _personRequestCheckAuthorization);
			}
		}

	}
}