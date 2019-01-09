using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.DaySchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
	public class ScheduleApiController : ApiController
	{
		private readonly IScheduleViewModelFactory _scheduleViewModelFactory;
		private readonly IScheduleDayViewModelFactory _scheduleDayViewModelFactory;
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IIntradayScheduleEdgeTimeCalculator _intradayScheduleEdgeTimeCalculator;
		private readonly IPushMessageProvider _pushMessageProvider;

		public ScheduleApiController(IScheduleViewModelFactory scheduleViewModelFactory, INow now, IUserTimeZone timeZone, IIntradayScheduleEdgeTimeCalculator intradayScheduleEdgeTimeCalculator, IPushMessageProvider pushMessageProvider, IScheduleDayViewModelFactory scheduleDayViewModelFactory)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
			_now = now;
			_timeZone = timeZone;
			_intradayScheduleEdgeTimeCalculator = intradayScheduleEdgeTimeCalculator;
			_pushMessageProvider = pushMessageProvider;
			_scheduleDayViewModelFactory = scheduleDayViewModelFactory;
		}

		[UnitOfWork, Route("api/Schedule/GetIntradayScheduleEdgeTime"), HttpGet]
		public virtual IntradayScheduleEdgeTime GetIntradayScheduleEdgeTime([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_timeZone.TimeZone());
			return _intradayScheduleEdgeTimeCalculator.GetSchedulePeriodForCurrentUser(showForDate);
		}

		[UnitOfWork, Route("api/Schedule/FetchDayData"), HttpGet]
		public virtual DayScheduleViewModel FetchDayData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_timeZone.TimeZone());
			return _scheduleDayViewModelFactory.CreateDayViewModel(showForDate, staffingPossiblityType);
		}

		[UnitOfWork, Route("api/Schedule/GetUnreadMessageCount"), HttpGet]
		public virtual int GetUnreadMessageCount()
		{
			return _pushMessageProvider.UnreadMessageCount;
		}

		[UnitOfWork, Route("api/Schedule/FetchWeekData"), HttpGet]
		public virtual WeekScheduleViewModel FetchWeekData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date, StaffingPossiblityType staffingPossiblityType = StaffingPossiblityType.None)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_timeZone.TimeZone());
			return _scheduleViewModelFactory.CreateWeekViewModel(showForDate, staffingPossiblityType);
		}

		[UnitOfWork, Route("api/Schedule/FetchMonthData"), HttpGet]
		public virtual MonthScheduleViewModel FetchMonthData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_timeZone.TimeZone());
			return _scheduleViewModelFactory.CreateMonthViewModel(showForDate);
		}

		[UnitOfWork, Route("api/Schedule/FetchMobileMonthData"), HttpGet]
		public virtual MonthScheduleViewModel FetchMobileMonthData([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly? date)
		{
			var showForDate = date ?? _now.CurrentLocalDate(_timeZone.TimeZone());
			return _scheduleViewModelFactory.CreateMobileMonthViewModel(showForDate);
		}
	}
}