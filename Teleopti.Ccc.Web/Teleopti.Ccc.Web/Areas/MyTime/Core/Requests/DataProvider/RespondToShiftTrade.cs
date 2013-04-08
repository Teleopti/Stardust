using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.Web.Core.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMappingEngine _mapper;
		private readonly IServiceBusSender _serviceBusSender;
		private readonly IUnitOfWorkFactoryProvider _unitOfWorkFactoryProvider;
		private readonly ICurrentBusinessUnitProvider _businessUnitProvider;
		private readonly INow _nu;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestSetChecksum;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository,
									IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
									IPersonRequestCheckAuthorization personRequestCheckAuthorization,
									ILoggedOnUser loggedOnUser,
									IMappingEngine mapper,
									IServiceBusSender serviceBusSender,
									IUnitOfWorkFactoryProvider unitOfWorkFactoryProvider,
									ICurrentBusinessUnitProvider businessUnitProvider,
									INow nu)
		{
			_personRequestRepository = personRequestRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_loggedOnUser = loggedOnUser;
			_mapper = mapper;
			_serviceBusSender = serviceBusSender;
			_unitOfWorkFactoryProvider = unitOfWorkFactoryProvider;
			_businessUnitProvider = businessUnitProvider;
			_nu = nu;
			_shiftTradeRequestSetChecksum = shiftTradeRequestSetChecksum;
		}

		public RequestViewModel OkByMe(Guid requestId)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}

			shouldNotBeHere(personRequest);

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

		private void shouldNotBeHere(IPersonRequest personRequest)
		{
			if (_serviceBusSender.EnsureBus())
			{
				_serviceBusSender.NotifyServiceBus(new AcceptShiftTrade
													   {
														   BusinessUnitId = _businessUnitProvider.CurrentBusinessUnit().Id.GetValueOrDefault(),
														   Datasource = _unitOfWorkFactoryProvider.LoggedOnUnitOfWorkFactory().Name,
														   Timestamp = _nu.UtcDateTime(),
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