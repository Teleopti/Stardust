using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePersonScheduleViewModelMapper : IShiftTradePersonScheduleViewModelMapper
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IShiftTradePersonScheduleProvider _personScheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;

		public ShiftTradePersonScheduleViewModelMapper(IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser, IShiftTradePersonScheduleProvider personScheduleProvider, ITeamScheduleProjectionProvider projectionProvider)
		{
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_personScheduleProvider = personScheduleProvider;
			_projectionProvider = projectionProvider;
		}

		public ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(inputData.ShiftTradeDate,
				_loggedOnUser.CurrentUser())
				? _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] { _loggedOnUser.CurrentUser() }).SingleOrDefault()
				: null;
			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(_loggedOnUser.CurrentUser(), myScheduleDay, true);
			return new ShiftTradeAddPersonScheduleViewModel(myScheduleViewModel);
		}
	}

	public interface IShiftTradePersonScheduleViewModelMapper
	{
		ShiftTradeAddPersonScheduleViewModel MakeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData);
	}
}