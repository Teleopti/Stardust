using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleCommandInput
	{
		public DateTime Date { get; set; }
		public IEnumerable<PersonSkillModel> People { get; set; }
	}
}