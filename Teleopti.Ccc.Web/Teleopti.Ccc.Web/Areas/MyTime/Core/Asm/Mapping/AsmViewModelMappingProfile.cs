using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping
{
	public class AsmViewModelMappingProfile : Profile
	{
		private readonly IProjectionProvider _projectionProvider;

		public AsmViewModelMappingProfile(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IEnumerable<IScheduleDay>, AsmViewModel>()
				.ForMember(d => d.Layers, o => o.MapFrom(source =>
				                                         	{
				                                         		var ret = new List<IVisualLayer>();
																			foreach (var proj in source.Select(scheduleDay => _projectionProvider.Projection(scheduleDay)))
																			{
																				ret.AddRange(proj);
																			}
				                                         		return ret;
				                                         	}));
			CreateMap<IVisualLayer, AsmLayer>()
				.ForMember(d => d.Payload, o => o.MapFrom(source => source.DisplayDescription()));

		}
	}
}