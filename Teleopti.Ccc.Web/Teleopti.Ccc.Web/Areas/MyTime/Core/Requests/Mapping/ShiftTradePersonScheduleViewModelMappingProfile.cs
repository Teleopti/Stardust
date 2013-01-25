using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePersonScheduleViewModelMappingProfile : Profile
	{
		private readonly Func<IProjectionProvider> _projectionProvider;

		public ShiftTradePersonScheduleViewModelMappingProfile(Func<IProjectionProvider> projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}
	}
}