using AutoMapper;

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