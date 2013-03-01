using System;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			return Mapper.Map<PersonScheduleData, PersonScheduleViewModel>(data);
		}
	}
}