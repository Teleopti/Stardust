using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModel
	{
		public string Name { get; set; }
		public string Site { get; set; }
		public string Team { get; set; }
		public bool IsDayOff { get; set; }
		public IEnumerable<PersonScheduleViewModelLayer> Layers { get; set; }
		public IEnumerable<PersonScheduleViewModelAbsence> Absences { get; private set; }
		public IEnumerable<PersonScheduleViewModelPersonAbsence> PersonAbsences  { get; private set; }
	}

	public class PersonScheduleViewModelPersonAbsence
	{
		public string Color { get; set; }
		public string Name { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Id { get; set; }
	}

	public class PersonScheduleViewModelAbsence
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}

	public class PersonScheduleViewModelLayer
	{
		public string Color { get; set; }
		public string Start { get; set; }
		public int Minutes { get; set; }
	}
}