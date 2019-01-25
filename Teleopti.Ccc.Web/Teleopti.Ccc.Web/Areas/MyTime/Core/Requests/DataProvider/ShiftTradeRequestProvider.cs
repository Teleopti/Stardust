using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder,
			IPermissionProvider permissionProvider, IScheduleStorage scheduleStorage, ICurrentScenario scenario)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_permissionProvider = permissionProvider;
			_scheduleStorage = scheduleStorage;
			_currentScenario = scenario;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date)
		{
			var person = _loggedOnUser.CurrentUser();
			return _permissionProvider.IsPersonSchedulePublished(date, person) ||
				   _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths
					   .ViewUnpublishedSchedules)
				? _scheduleDayReadModelFinder.ForPerson(date, person.Id.Value)
				: null;
		}

		public IScheduleDictionary RetrieveTradeMultiSchedules(DateOnlyPeriod period, IList<IPerson> personList)
		{
			return _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personList, new ScheduleDictionaryLoadOptions(false, false),
				period, _currentScenario.Current());
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging, string timeSortOrder = "")
		{
			var personIdList = possibleShiftTradePersons.Where(p => p.Id.HasValue).Select(p => p.Id.Value).ToList();
			var timeFilterInfo = new TimeFilterInfo
			{
				IsDayOff = true,
				IsWorkingDay = true,
				IsEmptyDay = true
			};

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

		public Guid? RetrieveMySiteId(DateOnly date)
		{
			var myTeam = _loggedOnUser.CurrentUser().MyTeam(date);
			var mySite = myTeam?.Site;
			if (mySite == null || !_permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, date, myTeam))
			{
				return null;
			}

			return mySite.Id;
		}
	}
}