using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestPersister : IShiftTradeRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestMapper _shiftTradeRequestMapper;
		private readonly IMappingEngine _autoMapper;
		private readonly INow _now;
		private readonly ICurrentDataSource _dataSourceProvider;
		private readonly ICurrentBusinessUnit _businessUnitProvider;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeSetChecksum;
		private readonly IShiftTradeRequestProvider _shiftTradeRequestprovider;

		public ShiftTradeRequestPersister(IPersonRequestRepository personRequestRepository,
			IShiftTradeRequestMapper shiftTradeRequestMapper,
			IMappingEngine autoMapper,
			IMessagePopulatingServiceBusSender serviceBusSender,
			INow now,
			ICurrentDataSource dataSourceProvider,
			ICurrentBusinessUnit businessUnitProvider,
			ICurrentUnitOfWork currentUnitOfWork,
			IShiftTradeRequestSetChecksum shiftTradeSetChecksum, IShiftTradeRequestProvider shiftTradeRequestprovider)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestMapper = shiftTradeRequestMapper;
			_autoMapper = autoMapper;
			_now = now;
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
			_currentUnitOfWork = currentUnitOfWork;
			_serviceBusSender = serviceBusSender;
			_shiftTradeSetChecksum = shiftTradeSetChecksum;
			_shiftTradeRequestprovider = shiftTradeRequestprovider;
		}

		public RequestViewModel Persist(ShiftTradeRequestForm form)
		{
			if (!isOfferAvailable(form))
			{
				return new RequestViewModel() {ExchangeOffer = new ShiftExchangeOfferRequestViewModel() {IsOfferAvailable = false}};
			}
		
			var personRequest = _shiftTradeRequestMapper.Map(form);
			_shiftTradeSetChecksum.SetChecksum(personRequest.Request);
			_personRequestRepository.Add(personRequest);

			createMessage(personRequest, form);
				
			var requestViewModel = _autoMapper.Map<IPersonRequest, RequestViewModel>(personRequest);
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId != null && workflowControlSet.LockTrading)
			{
				if (!workflowControlSet.AutoGrantShiftTradeRequest)
				{
					var offer = getOffer(form);
					offer.Status = ShiftExchangeOfferStatus.PendingAdminApproval;
				}
				requestViewModel.Status = Resources.WaitingThreeDots;
			}

			return requestViewModel;
		}

		private void createMessage(IPersonRequest personRequest, ShiftTradeRequestForm form)
		{
			if (_currentUnitOfWork == null)
				return;
			MessageWithLogOnInfo message = null;
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId != null && workflowControlSet.LockTrading)
			{
				var shiftTrade = personRequest.Request as IShiftTradeRequest;
				if (shiftTrade != null)
				{
					message = new AcceptShiftTrade
					{
						PersonRequestId = personRequest.Id.GetValueOrDefault(),
						AcceptingPersonId = shiftTrade.PersonTo.Id.GetValueOrDefault()
					};
				}
			}
			else
			{
				message = new NewShiftTradeRequestCreated
				{
					LogOnBusinessUnitId = _businessUnitProvider.Current().Id.GetValueOrDefault(Guid.Empty),
					LogOnDatasource = _dataSourceProvider.Current().DataSourceName,
					PersonRequestId = personRequest.Id.GetValueOrDefault(Guid.Empty),
					Timestamp = _now.UtcDateTime()
				};
			}
			_currentUnitOfWork.Current().AfterSuccessfulTx(() => _serviceBusSender.Send(message, false));
		}

		private IShiftExchangeOffer getOffer(ShiftTradeRequestForm form)
		{
			var personRequest = _personRequestRepository.FindPersonRequestByRequestId(form.ShiftExchangeOfferId.Value);
			return personRequest.Request as IShiftExchangeOffer;
		}

		private bool isOfferAvailable(ShiftTradeRequestForm form)
		{
			var workflowControlSet = _shiftTradeRequestprovider.RetrieveUserWorkflowControlSet();
			if (form.ShiftExchangeOfferId != null && workflowControlSet.LockTrading)
			{
				var offer = getOffer(form);
				if (offer == null) return true;
				return offer.Status != ShiftExchangeOfferStatus.Completed && offer.Status != ShiftExchangeOfferStatus.PendingAdminApproval;
			}

			return true;
		}
	}
}