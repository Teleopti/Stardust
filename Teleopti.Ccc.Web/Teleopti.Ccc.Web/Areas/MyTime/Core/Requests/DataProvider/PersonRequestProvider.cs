using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PersonRequestProvider : IPersonRequestProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IToggleManager _toggleManager;
		private readonly IPermissionProvider _permissionProvider;

		public PersonRequestProvider(IPersonRequestRepository repository, ILoggedOnUser loggedOnUser, IUserTimeZone userTimeZone, IToggleManager toggleManager, IPermissionProvider permissionProvider)
		{
			_repository = repository;
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
			_toggleManager = toggleManager;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<IPersonRequest> RetrieveRequests(Paging paging)
		{
			var types = new List<RequestType>
			{
				RequestType.AbsenceRequest,
				RequestType.ShiftExchangeOffer,
				RequestType.ShiftTradeRequest,
				RequestType.TextRequest
			};

			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb,
				DateOnly.Today, _loggedOnUser.CurrentUser()))
			{
				types.Remove(RequestType.ShiftExchangeOffer);
				types.Remove(RequestType.ShiftTradeRequest);
			}

			if (!_toggleManager.IsEnabled(Toggles.MyTimeWeb_SeeAnnouncedShifts_31639))
			{
				types.Remove(RequestType.ShiftExchangeOffer);
			}
			return _repository.FindAllRequestsForAgentByType(_loggedOnUser.CurrentUser(), paging, types.ToArray());

		}

		public IEnumerable<IPersonRequest> RetrieveRequests(DateOnlyPeriod period)
		{
			return _repository.FindAllRequestsForAgent(_loggedOnUser.CurrentUser(), period.ToDateTimePeriod(_userTimeZone.TimeZone()));
		}

		public IPersonRequest RetrieveRequest(Guid id)
		{
			var personRequest = _repository.Get(id);
			if (personRequest == null)
				throw new DataSourceException("No person request found for id " + id);
			return personRequest;
		}
	}
}