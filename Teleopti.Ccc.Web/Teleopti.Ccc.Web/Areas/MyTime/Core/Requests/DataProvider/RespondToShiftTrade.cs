using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IEventPublisher _publisher;
		private readonly INow _nu;
		private readonly ICurrentDataSource _dataSourceProvider;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
	    private readonly IEventSyncronization _eventSyncronization;
	    private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestSetChecksum;
		private readonly IToggleManager _toggleManager;
		private readonly RequestsViewModelMapper _mapper;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository,
			IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			ILoggedOnUser loggedOnUser,
			IEventPublisher publisher,
			INow nu,
			ICurrentDataSource dataSourceProvider,
			ICurrentBusinessUnit businessUnitProvider,
            IEventSyncronization eventSyncronization, IToggleManager toggleManager, RequestsViewModelMapper mapper)
		{
			_personRequestRepository = personRequestRepository;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_loggedOnUser = loggedOnUser;
			_publisher = publisher;
			_nu = nu;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
            _eventSyncronization = eventSyncronization;
	        _toggleManager = toggleManager;
			_mapper = mapper;
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
            _eventSyncronization.WhenDone(() => persistWithBus(personRequest));
           return _mapper.Map(personRequest);
		}

		public RequestViewModel Deny(Guid requestId, string message)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}
			personRequest.TrySetMessage(message);
			personRequest.Deny(nameof(Resources.RequestDenyReasonOtherPart), _personRequestCheckAuthorization, _loggedOnUser.CurrentUser());

			return _mapper.Map(personRequest);
		}

		public RequestViewModel ResendReferred(Guid requestId)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			if (personRequest == null)
			{
				return new RequestViewModel();
			}

			personRequest.TrySetMessage(Resources.ShiftTradeResendMessage);

			((IShiftTradeRequest)personRequest.Request).Accept(personRequest.Person, _shiftTradeRequestSetChecksum, _personRequestCheckAuthorization);

            _eventSyncronization.WhenDone(() => _publisher.Publish(new NewShiftTradeRequestCreatedEvent
            {
                LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
                LogOnDatasource = _dataSourceProvider.Current().DataSourceName,
                PersonRequestId = requestId,
                Timestamp = _nu.UtcDateTime()
            }));

            return _mapper.Map(personRequest);
		}

		private void persistWithBus(IPersonRequest personRequest)
		{
			_publisher.Publish(new AcceptShiftTradeEvent
			{
				LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
				LogOnDatasource = _dataSourceProvider.CurrentName(),
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				AcceptingPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				Message = personRequest.GetMessage(new NoFormatting()),
				UseSiteOpenHoursRule = true,
				UseMaximumWorkday = true
			});
		}

	}
}