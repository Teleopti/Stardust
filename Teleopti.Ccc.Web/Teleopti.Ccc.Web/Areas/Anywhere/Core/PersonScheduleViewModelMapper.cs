using AutoMapper;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			var map = Mapper.Map<PersonScheduleData, PersonScheduleViewModel>(data);
			map.Name = data.CommonAgentNameSetting.BuildCommonNameDescription(data.Person);
			return map;
		}
	}
}