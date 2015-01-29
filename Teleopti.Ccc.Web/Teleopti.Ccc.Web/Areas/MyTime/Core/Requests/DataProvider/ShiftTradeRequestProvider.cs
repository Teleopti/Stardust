using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser, IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPermissionProvider permissionProvider, IToggleManager toggleManager)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date)
		{
			var person = _loggedOnUser.CurrentUser();
			if (_permissionProvider.IsPersonSchedulePublished(date, person))
				return _scheduleDayReadModelFinder.ForPerson(date, person.Id.Value);
			return null;
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging)
		{
			IEnumerable<Guid> personIdList = (from person in possibleShiftTradePersons
				select person.Id.Value).ToList();
			if (_toggleManager.IsEnabled(Toggles.Request_ShiftTradeWithEmptyDays_28926))
				return _scheduleDayReadModelFinder.ForPersonsIncludeEmptyDays(date, personIdList, paging);

			return _scheduleDayReadModelFinder.ForPersons(date, personIdList, paging);
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrieveBulletinTradeSchedules(IEnumerable<string> shiftExchangeOfferIds , Paging paging)
		{			
			return _scheduleDayReadModelFinder.ForBulletinPersons(shiftExchangeOfferIds, paging);
		}		

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedulesWithFilteredTimes(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging,
																						TimeFilterInfo filterInfo)
		{
			IEnumerable<Guid> personIdList = (from person in possibleShiftTradePersons
			                                 select person.Id.Value).ToList();
			return _scheduleDayReadModelFinder.ForPersonsByFilteredTimes(date, personIdList, paging, filterInfo);
		}

		public Guid? RetrieveMyTeamId(DateOnly date)
		{
			var myTeam = _loggedOnUser.CurrentUser().MyTeam(date);
			if (myTeam == null || !_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, date, myTeam))
				return null;

			return myTeam.Id;
		}
	}
}