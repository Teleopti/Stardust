using System.Drawing;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping
{
	public class CommonViewModelMappingProfile : Profile
	{
		protected override void Configure()
		{
			CreateMap<Color, StyleClassViewModel>()
				.ForMember(d => d.Name, o => o.MapFrom(s => s.ToStyleClass()))
				.ForMember(d => d.ColorHex, o => o.MapFrom(s => s.ToHtml()))
				;
		}
	}
}