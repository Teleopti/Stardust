using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersister : IShiftTradeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestMapper _shiftTradeRequestMapper;
		private readonly IEventPublisher _publisher;
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSourceProvider;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeSetChecksum;
		private readonly IShiftTradeRequestProvider _shiftTradeRequestprovider;
		private readonly IShiftTradeRequestPersonToPermissionValidator _shiftTradeRequestPermissionValidator;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly RequestsViewModelMapper _mapper;
		private readonly IToggleManager _toggleManager;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository,
			IShiftTradeRequestMapper shiftTradeRequestMapper,
			IEventPublisher publisher,
			INow now,
			ICurrentDataSource dataSourceProvider,
			ICurrentBusinessUnit businessUnitProvider,
			ICurrentUnitOfWork currentUnitOfWork,
			IShiftTradeRequestSetChecksum shiftTradeSetChecksum,
			IShiftTradeRequestProvider shiftTradeRequestprovider,
			IShiftTradeRequestPersonToPermissionValidator shiftTradePersonToPermissionValidator,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			RequestsViewModelMapper mapper, IToggleManager toggleManager)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
			_publisher = publisher;
			_now = now;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
			_currentUnitOfWork = currentUnitOfWork;
			_shiftTradeSetChecksum = shiftTradeSetChecksum;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
			_shiftTradeRequestPermissionValidator = shiftTradePersonToPermissionValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_mapper = mapper;
			_toggleManager = toggleManager;
		}

		public RequestViewModel Persist(ShiftTradeRequestForm form)
		{
			if (!isOfferAvailable(form))
			{
				return new RequestViewModel
				{
					ExchangeOffer = new ShiftExchangeOfferRequestViewModel
					{
						IsOfferAvailable = false
					}
				};
			}

			var personRequest = _shiftTradeRequestMapper.Map(form);
			_shiftTradeSetChecksum.SetChecksum(personRequest.Request);
			_personRequestRepository.Add(personRequest);

			var permissionSatisfied =
				_shiftTradeRequestPermissionValidator.IsSatisfied(personRequest.Request as IShiftTradeRequest);
			if (!permissionSatisfied)
			{
				personRequest.Deny(nameof(Resources.RecipientHasNoShiftTradePermission), _personRequestCheckAuthorization);
			}

			var requestViewModel = _mapper.Map(personRequest);
			if (!permissionSatisfied)
			{
				return requestViewModel;
			}

			createMessage(personRequest, form);

			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId == null || !workflowControlSet.LockTrading)
			{
				return requestViewModel;
			}

			if (!workflowControlSet.AutoGrantShiftTradeRequest)
			{
				var offer = getOffer(form);
				offer.Status = ShiftExchangeOfferStatus.PendingAdminApproval;
			}
			requestViewModel.Status = Resources.WaitingThreeDots;

			return requestViewModel;
		}

		private void createMessage(IPersonRequest personRequest, ShiftTradeRequestForm form)
		{
			if (_currentUnitOfWork == null)
				return;
			IEvent @event = null;
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId != null && workflowControlSet.LockTrading)
			{
				var shiftTrade = personRequest.Request as IShiftTradeRequest;
				if (shiftTrade != null)
				{
					@event = new AcceptShiftTradeEvent
					{
						LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
						LogOnDatasource = _dataSourceProvider.Current().DataSourceName,
						PersonRequestId = personRequest.Id.GetValueOrDefault(),
						AcceptingPersonId = shiftTrade.PersonTo.Id.GetValueOrDefault(),
						UseSiteOpenHoursRule = true
					};
				}
			}
			else
			{
				@event = new NewShiftTradeRequestCreatedEvent
				{
					LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					LogOnDatasource = _dataSourceProvider.Current().DataSourceName,
					PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime()
				};
			}
			_currentUnitOfWork.Current().AfterSuccessfulTx(() => _publisher.Publish(@event));
		}

		private IShiftExchangeOffer getOffer(ShiftTradeRequestForm form)
		{
			var shiftExchangeOfferId = form.ShiftExchangeOfferId.Value;
			var personRequest = _personRequestRepository.FindPersonRequestByRequestId(shiftExchangeOfferId);
			return personRequest.Request as IShiftExchangeOffer;
		}

		private bool isOfferAvailable(ShiftTradeRequestForm form)
		{
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId == null || !workflowControlSet.LockTrading)
			{
				return true;
			}

			var offer = getOffer(form);
			if (offer == null)
			{
				return  true;
			}

			return offer.Status != ShiftExchangeOfferStatus.Completed &&
				   offer.Status != ShiftExchangeOfferStatus.PendingAdminApproval;
		}
	}
}