using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestCheckSum;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IMappingEngine _mapper;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository, IShiftTradeRequestSetChecksum shiftTradeRequestCheckSum, IPersonRequestCheckAuthorization personRequestCheckAuthorization, ILoggedOnUser loggedOnUser, IMappingEngine mapper)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestCheckSum = shiftTradeRequestCheckSum;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_loggedOnUser = loggedOnUser;
			_mapper = mapper;
		}

		public void OkByMe(Guid requestId)
		{
			var request = _personRequestRepository.Find(requestId);
			var shiftTrade = request.Request as IShiftTradeRequest;
			shiftTrade.Accept(_loggedOnUser.CurrentUser(), _shiftTradeRequestCheckSum, _personRequestCheckAuthorization);
		}

		public RequestViewModel Deny(Guid requestId)
		{
			var personRequest = _personRequestRepository.Find(requestId);
			personRequest.TrySetMessage(personRequest.GetMessage(new NoFormatting()));
			personRequest.Deny(_loggedOnUser.CurrentUser(), "RequestDenyReasonOtherPart", _personRequestCheckAuthorization);

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}