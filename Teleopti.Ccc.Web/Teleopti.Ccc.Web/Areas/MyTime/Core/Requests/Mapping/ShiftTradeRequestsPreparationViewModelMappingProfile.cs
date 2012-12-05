using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestsPreparationViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			base.Configure();

			CreateMap<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>()
				.ForMember(d => d.HasWorkflowControlSet, o => o.MapFrom(s => s.WorkflowControlSet != null))
				;
		}
	}
}