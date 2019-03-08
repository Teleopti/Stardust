using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
	public class ScheduleStaffingPossibilityController : ApiController
	{
		private readonly IStaffingPossibilityViewModelFactory _staffingPossibilityViewModelFactory;
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;

		public ScheduleStaffingPossibilityController(IStaffingPossibilityViewModelFactory staffingPossibilityViewModelFactory,
			INow now, IUserTimeZone userTimeZone)
		{
			_staffingPossibilityViewModelFactory = staffingPossibilityViewModelFactory;
			_now = now;
			_userTimeZone = userTimeZone;
		}

		[UnitOfWork, Route("api/ScheduleStaffingPossibility"), HttpGet]
		public virtual IEnumerable<PeriodStaffingPossibilityViewModel> GetPossibilityViewModels(
			[ModelBinder(typeof(DateOnlyModelBinder))] DateOnly? date,
			StaffingPossibilityType staffingPossibilityType = StaffingPossibilityType.None,
			bool returnOneWeekData = true)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_userTimeZone.TimeZone());
			return
				_staffingPossibilityViewModelFactory.CreatePeriodStaffingPossibilityViewModels(
					date.GetValueOrDefault(showForDate), staffingPossibilityType, returnOneWeekData);
		}
	}
}
