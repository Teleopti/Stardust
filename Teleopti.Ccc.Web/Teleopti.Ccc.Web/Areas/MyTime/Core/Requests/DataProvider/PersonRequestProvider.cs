using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PersonRequestProvider : IPersonRequestProvider
	{
		private readonly IPersonRequestRepository _repository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IToggleManager _toggleManager;

		public PersonRequestProvider(IPersonRequestRepository repository, ILoggedOnUser loggedOnUser, IUserTimeZone userTimeZone, IToggleManager toggleManager)
		{
			_repository = repository;
			_loggedOnUser = loggedOnUser;
			_userTimeZone = userTimeZone;
			_toggleManager = toggleManager;
		}

		public IEnumerable<IPersonRequest> RetrieveRequests(Paging paging)
		{
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_SeeAnnouncedShifts_31639))
			{
				return _repository.FindAllRequestsForAgent(_loggedOnUser.CurrentUser(), paging);
			}
			else
			{
				return _repository.FindAllRequestsExceptOffer(_loggedOnUser.CurrentUser(), paging);
			}
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