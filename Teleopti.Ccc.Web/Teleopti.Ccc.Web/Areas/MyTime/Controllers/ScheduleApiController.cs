using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
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

		[UnitOfWork, Route("api/Schedule/FetchData"), HttpGet]
		public virtual WeekScheduleViewModel FetchData(DateOnly? date)
		{
			var showForDate = date ?? _now.LocalDateOnly();
			return _scheduleViewModelFactory.CreateWeekViewModel(showForDate);
		}

		[UnitOfWork, Route("api/Schedule/FetchMonthData"), HttpGet]
		public virtual MonthScheduleViewModel FetchMonthData(DateOnly? date)
		{
			var showForDate = date ?? _now.LocalDateOnly();
			return _scheduleViewModelFactory.CreateMonthViewModel(showForDate);
		}
	}
}