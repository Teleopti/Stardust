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
				.ConvertUsing(s =>
					              {
						              var details = s.ShiftTradeSwapDetails.First();


						              var viewModel = new ShiftTradeSwapDetailsViewModel()
							                              {
								                              DateFrom = details.DateFrom,
								                              DateTo = details.DateTo
							                              };
						              return viewModel;
					              });
		}
	}
}