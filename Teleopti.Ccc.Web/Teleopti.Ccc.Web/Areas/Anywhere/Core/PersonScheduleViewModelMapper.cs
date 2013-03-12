using System;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			var viewModel = Mapper.Map<PersonScheduleData, PersonScheduleViewModel>(data);
			if (viewModel.Layers != null)
			{
				viewModel.Layers.ForEach(l =>
				                         	{
				                         		if (l.Start != DateTime.MinValue)
				                         			l.Start = TimeZoneInfo.ConvertTimeFromUtc(l.Start,
				                         			                                          data.Person.PermissionInformation.
				                         			                                          	DefaultTimeZone());
				                         	});
			}
			return viewModel;
		}
	}
}