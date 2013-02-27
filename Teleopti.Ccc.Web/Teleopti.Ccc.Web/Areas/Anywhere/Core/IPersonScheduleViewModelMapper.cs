using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{

	public class PersonScheduleViewModel
	{
		public string Name { get; set; }
		public string Site { get; set; }
		public string Team { get; set; }
	}

	public class PersonScheduleData
	{
		public IPerson Person { get; set; }
	}

	public interface IPersonScheduleViewModelMapper
	{
		PersonScheduleViewModel Map(PersonScheduleData data);
	}

	public class PersonScheduleViewModelMapper : IPersonScheduleViewModelMapper
	{
		public PersonScheduleViewModel Map(PersonScheduleData data)
		{
			var viewModel = new PersonScheduleViewModel {Name = data.Person.Name.ToString()};
			return viewModel;
		}
	}

}