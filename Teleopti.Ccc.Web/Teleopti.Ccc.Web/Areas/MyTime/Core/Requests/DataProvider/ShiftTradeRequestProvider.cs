using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPermissionProvider _permissionProvider;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder,
			IPermissionProvider permissionProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_permissionProvider = permissionProvider;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date)
		{
			var person = _loggedOnUser.CurrentUser();
			return _permissionProvider.IsPersonSchedulePublished(date, person)
				? _scheduleDayReadModelFinder.ForPerson(date, person.Id.Value)
				: null;
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging, string timeSortOrder = "")
		{
			var personIdList = (from person in possibleShiftTradePersons  where person.Id.HasValue select person.Id.Value).ToList();

			var timeFilterInfo = new TimeFilterInfo() {IsDayOff = true, IsWorkingDay = true, IsEmptyDay = true};
			return _scheduleDayReadModelFinder.ForPersons(date, personIdList, paging, timeFilterInfo, timeSortOrder);

		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrieveBulletinTradeSchedules(
			IEnumerable<string> shiftExchangeOfferIds, Paging paging, string timeSortOrder = "")
		{
			return _scheduleDayReadModelFinder.ForBulletinPersons(shiftExchangeOfferIds, paging);
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedulesWithFilteredTimes(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging,
			TimeFilterInfo filterInfo, string timeSortOrder = "")
		{
			var personIdList = (from person in possibleShiftTradePersons where person.Id.HasValue select person.Id.Value).ToList();
			return _scheduleDayReadModelFinder.ForPersons(date, personIdList, paging, filterInfo, timeSortOrder);
		}

		public Guid? RetrieveMyTeamId(DateOnly date)
		{
			var myTeam = _loggedOnUser.CurrentUser().MyTeam(date);
			if (myTeam == null ||
			    !_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, date, myTeam))
			{
				return null;
			}

			return myTeam.Id;
		}
	}
}