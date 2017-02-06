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

		[UnitOfWork, Route("api/IntradayStaffingPossibility/Absence"), HttpGet]
		public virtual StaffingPossibilityViewModel GetIntradayAbsencePossibility()
		{
			return _staffingPossibilityViewModelFactory.CreateIntradayAbsencePossibilityViewModel();
		}

		[UnitOfWork, Route("api/IntradayStaffingPossibility/Overtime"), HttpGet]
		public virtual StaffingPossibilityViewModel GetIntradayOvertimePossibility()
		{
			return _staffingPossibilityViewModelFactory.CreateIntradayOvertimePossibilityViewModel();
		}
	}
}
