using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class PeopleSkillCommandInput
	{
		public DateTime Date { get; set; }
		public IEnumerable<SkillUpdateCommandInputModel> People { get; set; }
	}
}