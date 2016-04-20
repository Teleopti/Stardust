using System;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMappingEngine _mapper;
		private readonly IEventPublisher _publisher;
		private readonly INow _nu;
		private readonly ICurrentDataSource _dataSourceProvider;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestSetChecksum;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository,
			IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			ILoggedOnUser loggedOnUser,
			IMappingEngine mapper,
			IEventPublisher publisher,
			INow nu,
			ICurrentDataSource dataSourceProvider,
			ICurrentBusinessUnit businessUnitProvider)
		{
			_personRequestRepository = personRequestRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_loggedOnUser = loggedOnUser;
			_mapper = mapper;
			_publisher = publisher;
			_nu = nu;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
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

		public RequestViewModel Deny(Guid requestId, string message)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}
			personRequest.TrySetMessage(message);
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
			_publisher.Publish(new NewShiftTradeRequestCreatedEvent
			{
				LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
				LogOnDatasource = _dataSourceProvider.Current().DataSourceName,
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				Timestamp = _nu.UtcDateTime()
			});

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}

		private void persistWithBus(IPersonRequest personRequest)
		{
			_publisher.Publish(new AcceptShiftTradeEvent
			{
				LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
				LogOnDatasource = _dataSourceProvider.CurrentName(),
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				AcceptingPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				Message = personRequest.GetMessage(new NoFormatting())
			});
		}

	}
}