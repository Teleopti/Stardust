using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleSkillCommandInput
	{
		public DateTime Date { get; set; }
		public IEnumerable<SkillUpdateCommandInputModel> People { get; set; }
	}
	public class PeopleShiftBagCommandInput
	{
		public DateTime Date { get; set; }
		public List<ShiftBagUpdateCommandInputModel> People { get; set; }
	}
}