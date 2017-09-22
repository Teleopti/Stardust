using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
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
		private readonly IUserTimeZone _timeZone;
		private readonly IIntradayScheduleEdgeTimeCalculator _intradayScheduleEdgeTimeCalculator;

		public ScheduleApiController(IScheduleViewModelFactory scheduleViewModelFactory, INow now, IUserTimeZone timeZone, IIntradayScheduleEdgeTimeCalculator intradayScheduleEdgeTimeCalculator)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
			_now = now;
			_timeZone = timeZone;
			_intradayScheduleEdgeTimeCalculator = intradayScheduleEdgeTimeCalculator;
		}

		[UnitOfWork, Route("api/Schedule/GetIntradayScheduleEdgeTime"), HttpGet]
		public virtual IntradayScheduleEdgeTime GetIntradayScheduleEdgeTime([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var nowForUser = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var showForDate = date ?? new DateOnly(nowForUser.Date);

			var period = _intradayScheduleEdgeTimeCalculator.GetSchedulePeriodForCurrentUser(showForDate);

			return period;
		}

		[UnitOfWork, Route("api/Schedule/FetchDayData"), HttpGet]
		public virtual DayScheduleViewModel FetchDayData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None)
		{
			var nowForUser = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var showForDate = date ?? new DateOnly(nowForUser.Date);
			
			return _scheduleViewModelFactory.CreateDayViewModel(showForDate, staffingPossiblityType);
		}

		[UnitOfWork, Route("api/Schedule/FetchWeekData"), HttpGet]
		public virtual WeekScheduleViewModel FetchWeekData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None)
		{
			var nowForUser = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var showForDate = date ?? new DateOnly(nowForUser.Date);
			
			return _scheduleViewModelFactory.CreateWeekViewModel(showForDate, staffingPossiblityType);
		}

		[UnitOfWork, Route("api/Schedule/FetchMonthData"), HttpGet]
		public virtual MonthScheduleViewModel FetchMonthData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var nowForUser = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var showForDate = date ?? new DateOnly(nowForUser.Date);
			return _scheduleViewModelFactory.CreateMonthViewModel(showForDate);
		}
	}
}