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
			var requests = _toggleManager.IsEnabled(Toggles.MyTimeWeb_SeeAnnouncedShifts_31639) ?
				_repository.FindAllRequestsForAgent(_loggedOnUser.CurrentUser(), paging) :
				_repository.FindAllRequestsExceptOffer(_loggedOnUser.CurrentUser(), paging);

			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, DateOnly.Today, _loggedOnUser.CurrentUser()))
			{
				requests = requests.Where(request => request.Request.RequestType != RequestType.ShiftTradeRequest);
			}

			return requests;
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