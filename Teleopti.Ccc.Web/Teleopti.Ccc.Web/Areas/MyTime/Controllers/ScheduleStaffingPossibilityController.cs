using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
	public class ScheduleStaffingPossibilityController : ApiController
	{
		private readonly IStaffingPossibilityViewModelFactory _staffingPossibilityViewModelFactory;

		public ScheduleStaffingPossibilityController(IStaffingPossibilityViewModelFactory staffingPossibilityViewModelFactory)
		{
			_staffingPossibilityViewModelFactory = staffingPossibilityViewModelFactory;
		}

		[UnitOfWork, Route("api/ScheduleStaffingPossibility/Absence"), HttpGet]
		public virtual StaffingPossibilityViewModel GetAbsencePossibilityForIntraday()
		{
			return _staffingPossibilityViewModelFactory.CreateIntradayAbsencePossibilityViewModel();
		}

		[UnitOfWork, Route("api/ScheduleStaffingPossibility/Overtime"), HttpGet]
		public virtual StaffingPossibilityViewModel GetOvertimePossibilityForIntraday()
		{
			return _staffingPossibilityViewModelFactory.CreateIntradayOvertimePossibilityViewModel();
		}
	}
}
