using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftTradeRequestApprovalService : IRequestApprovalService
	{
		private readonly ISwapAndModifyService _swapAndModifyService;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly INewBusinessRuleCollection _newBusinessRules;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IPersonRequestRepository _personRequestRepository;

		public ShiftTradeRequestApprovalService(IScheduleDictionary scheduleDictionary,
			ISwapAndModifyService swapAndModifyService,
			INewBusinessRuleCollection newBusinessRules,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IPersonRequestRepository personRequestRepository)
		{
			_swapAndModifyService = swapAndModifyService;
			_scheduleDictionary = scheduleDictionary;
			_newBusinessRules = newBusinessRules;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_personRequestRepository = personRequestRepository;
		}

		public bool CheckShiftTradeExchangeOffer { get; set; } = true;

		public IEnumerable<IBusinessRuleResponse> Approve(IRequest request)
		{
			var shiftTradeRequest = request as IShiftTradeRequest;
			if (shiftTradeRequest == null)
			{
				throw new InvalidCastException("Request type should be ShiftTradeRequest!");
			}

			var shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(_scheduleDictionary, _personRequestCheckAuthorization);
			var shiftTradeStatus = shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
			if (shiftTradeStatus == ShiftTradeStatus.Referred)
			{
				var person = shiftTradeRequest.PersonFrom;
				return new []
				{
					new BusinessRuleResponse(null, Resources.TheScheduleHasChanged, true, true, shiftTradeRequest.Period
						, person, shiftTradeRequest.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone()), string.Empty)
				};
			}

			var responses = _swapAndModifyService.SwapShiftTradeSwapDetails(shiftTradeRequest.ShiftTradeSwapDetails,
																  _scheduleDictionary,
																   _newBusinessRules, new ScheduleTagSetter(NullScheduleTag.Instance)).ToList();

			if (needToDenyOtherRequests(shiftTradeRequest, responses))
			{
				denyOtherRequests(request, shiftTradeRequest);
			}

			return responses;
		}

		private void denyOtherRequests(IRequest request, IShiftTradeRequest shiftTradeRequest)
		{
			var shiftTradeRequests =
				_personRequestRepository.FindShiftTradeRequestsByOfferId(shiftTradeRequest.Offer.Id.GetValueOrDefault());
			var otherShiftTradeRequests = shiftTradeRequests.Where(a => !a.Id.Equals(request.Parent.Id.GetValueOrDefault()));
			foreach (var otherShiftTradeRequest in otherShiftTradeRequests)
			{
				otherShiftTradeRequest.Deny(nameof(Resources.ShiftTradeRequestForExchangeOfferHasBeenCompleted),
					_personRequestCheckAuthorization);
			}
		}

		private bool needToDenyOtherRequests(IShiftTradeRequest shiftTradeRequest, List<IBusinessRuleResponse> responses)
		{
			return CheckShiftTradeExchangeOffer && shiftTradeRequest.Offer != null && responses.Count == 0;
		}
	}
}
