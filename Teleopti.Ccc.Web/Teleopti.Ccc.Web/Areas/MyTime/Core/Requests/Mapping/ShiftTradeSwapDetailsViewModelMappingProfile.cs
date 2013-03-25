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
				.ForMember(d=>d.To, o=>o.NullSubstitute(new ShiftTradePersonScheduleViewModel()))
				.ForMember(d=>d.From, o=>o.NullSubstitute(new ShiftTradePersonScheduleViewModel()))
				.ForMember(d => d.PersonFrom, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().PersonFrom.Name.ToString()))
				.ForMember(d => d.PersonTo, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().PersonTo.Name.ToString()))
				.ForMember(d => d.To, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartTo))
				.ForMember(d => d.From, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartFrom))
				.ForMember(d=> d.TimeLineHours, o=>o.MapFrom(s=> _timelineViewModelFactory.Invoke().CreateTimeLineHours(createTimelinePeriod(s))))
				.ForMember(d=> d.TimeLineStartDateTime, o=>o.MapFrom(s=> createTimelinePeriod(s).StartDateTime));
		}

		private static DateTimePeriod createTimelinePeriod(IShiftTradeRequest shiftTradeRequest)
		{
			var schedpartFrom = shiftTradeRequest.ShiftTradeSwapDetails.First().SchedulePartFrom;
			var schedpartTo = shiftTradeRequest.ShiftTradeSwapDetails.First().SchedulePartTo;
			if (schedpartFrom == null || schedpartTo == null)
			{
				//RK - when will this happen?
				return shiftTradeRequest.Period;
			}
			const int extraHourBeforeAndAfter = 1;
			DateTimePeriod totalPeriod;
			var fromTotalPeriod = shiftTradeRequest.ShiftTradeSwapDetails.First().SchedulePartFrom.TotalPeriod();
			var toTotalPeriod = shiftTradeRequest.ShiftTradeSwapDetails.First().SchedulePartTo.TotalPeriod();
			if (fromTotalPeriod.HasValue && toTotalPeriod.HasValue)
			{
				totalPeriod = fromTotalPeriod.Value.MaximumPeriod(toTotalPeriod.Value);
			}
			else if (fromTotalPeriod.HasValue)
			{
				totalPeriod = fromTotalPeriod.Value;
			}
			else if(toTotalPeriod.HasValue)
			{
				totalPeriod = toTotalPeriod.Value;					
			}
			else
			{
				totalPeriod = shiftTradeRequest.Period;
			}
			return new DateTimePeriod(totalPeriod.StartDateTime.AddHours(-extraHourBeforeAndAfter), 
			                          totalPeriod.EndDateTime.AddHours(extraHourBeforeAndAfter));
		}
	}
}