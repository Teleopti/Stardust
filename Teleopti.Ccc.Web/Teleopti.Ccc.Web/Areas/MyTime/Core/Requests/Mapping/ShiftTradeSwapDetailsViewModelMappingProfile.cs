using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeSwapDetailsViewModelMappingProfile : Profile
	{
		private readonly IResolve<IShiftTradeTimeLineHoursViewModelFactory> _timelineViewModelFactory;

		public ShiftTradeSwapDetailsViewModelMappingProfile(IResolve<IShiftTradeTimeLineHoursViewModelFactory> timelineViewModelFactory)
		{
			_timelineViewModelFactory = timelineViewModelFactory;
		}

		protected override void Configure()
		{
			CreateMap<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>()
				.ForMember(d => d.To, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartTo))
				.ForMember(d => d.From, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartFrom))
				.ForMember(d=> d.TimeLineHours, o=>o.MapFrom(s=> _timelineViewModelFactory.Invoke().CreateTimeLineHours(createTimelinePeriod(s))))
				.ForMember(d=> d.TimeLineStartDateTime, o=>o.MapFrom(s=> createTimelinePeriod(s).StartDateTime));
		}

		private static DateTimePeriod createTimelinePeriod(IShiftTradeRequest shiftTRadeRequest)
		{
			var fromPeriod = shiftTRadeRequest.ShiftTradeSwapDetails.First().SchedulePartFrom.Period;
			var toPeriod = shiftTRadeRequest.ShiftTradeSwapDetails.First().SchedulePartTo.Period;
			return fromPeriod.MaximumPeriod(toPeriod);
		}
	}
}