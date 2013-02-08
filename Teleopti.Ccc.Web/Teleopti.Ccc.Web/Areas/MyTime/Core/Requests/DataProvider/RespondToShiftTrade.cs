using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class RespondToShiftTrade : IRespondToShiftTrade
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IShiftTradeRequestSetChecksum _shiftTradeRequestCheckSum;
		private readonly IPersonRequestCheckAuthorization _personRequextCheckAuthorization;
		private readonly ILoggedOnUser _loggedOnUser;

		public RespondToShiftTrade(IPersonRequestRepository personRequestRepository, IShiftTradeRequestSetChecksum shiftTradeRequestCheckSum, IPersonRequestCheckAuthorization personRequextCheckAuthorization, ILoggedOnUser loggedOnUser)
		{
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestCheckSum = shiftTradeRequestCheckSum;
			_personRequextCheckAuthorization = personRequextCheckAuthorization;
			_loggedOnUser = loggedOnUser;
		}

		public void OkByMe(Guid requestId)
		{
			var request = _personRequestRepository.Find(requestId);
			var shiftTrade = request.Request as IShiftTradeRequest;
			shiftTrade.Accept(_loggedOnUser.CurrentUser(), _shiftTradeRequestCheckSum, _personRequextCheckAuthorization);
		}

		public void Deny(Guid requestId)
		{
			var request = _personRequestRepository.Find(requestId);
			var shiftTrade = request.Request as IShiftTradeRequest;
			shiftTrade.Deny(_loggedOnUser.CurrentUser());
		}
	}
}