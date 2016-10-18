using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PersonRequestProvider : IPersonRequestProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTimeZone _userTimeZone;

		private readonly IPermissionProvider _permissionProvider;

		public PersonRequestProvider(IPersonRequestRepository repository, ILoggedOnUser loggedOnUser, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider)
		{
			_repository = repository;
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(Paging paging, bool hideOldRequest)
		{
			var types = new List<RequestType>
			{
				RequestType.AbsenceRequest,
				RequestType.TextRequest
			};

			var hasPermissionForShiftTrade = _permissionProvider.HasPersonPermission(
				DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today,
				_loggedOnUser.CurrentUser());
			if (hasPermissionForShiftTrade)
			{
				types.Add(RequestType.ShiftExchangeOffer);
				types.Add(RequestType.ShiftTradeRequest);
			}

			DateTime? earliestDateUtc = null;
			if (hideOldRequest)
			{
				var currentTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				var earliestDateLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, currentTimezone).AddDays(-10).Date;
				earliestDateUtc = TimeZoneInfo.ConvertTimeToUtc(earliestDateLocal, currentTimezone);
			}

			return _repository.FindAllRequestsForAgentByType(_loggedOnUser.CurrentUser(), paging,
				earliestDateUtc, types.ToArray());
		}

		public IEnumerable<IPersonRequest> RetrieveRequestsForLoggedOnUser(DateOnlyPeriod period)
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