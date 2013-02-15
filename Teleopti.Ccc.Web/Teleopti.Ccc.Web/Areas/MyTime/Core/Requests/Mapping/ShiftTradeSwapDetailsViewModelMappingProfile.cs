using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeSwapDetailsViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			CreateMap<IShiftTradeRequest, ShiftTradeSwapDetailsViewModel>()
				.ForMember(d => d.To, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartTo))
				.ForMember(d => d.From, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().SchedulePartFrom))
			   .ForMember(d => d.DateFrom, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().DateFrom.Date))
				.ForMember(d => d.DateTo, o => o.MapFrom(s => s.ShiftTradeSwapDetails.First().DateTo.Date));
		}
	}
}