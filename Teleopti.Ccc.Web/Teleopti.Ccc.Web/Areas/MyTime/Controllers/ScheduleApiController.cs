using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
	public class ScheduleApiController : ApiController
	{
		private readonly IScheduleViewModelFactory _scheduleViewModelFactory;
		private readonly INow _now;

		public ScheduleApiController(IScheduleViewModelFactory scheduleViewModelFactory, INow now)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
			_now = now;
		}

		[UnitOfWork, Route("api/Schedule/FetchWeekData"), HttpGet]
		public virtual WeekScheduleViewModel FetchWeekData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None)
		{
			var showForDate = date ?? _now.LocalDateOnly();
			return _scheduleViewModelFactory.CreateWeekViewModel(showForDate, staffingPossiblityType);
		}

		[UnitOfWork, Route("api/Schedule/FetchMonthData"), HttpGet]
		public virtual MonthScheduleViewModel FetchMonthData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var showForDate = date ?? _now.LocalDateOnly();
			return _scheduleViewModelFactory.CreateMonthViewModel(showForDate);
		}
	}
}