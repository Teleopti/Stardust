using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPermissionProvider _permissionProvider;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser, IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPermissionProvider permissionProvider)
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
			return _scheduleDayReadModelFinder.ForPerson(date, person.Id.Value);
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging)
		{
			IEnumerable<Guid> personIdList = (from person in possibleShiftTradePersons
			                                 select person.Id.Value).ToList();
			return _scheduleDayReadModelFinder.ForPersons(date, personIdList, paging);
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