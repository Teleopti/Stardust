using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PersonSkillModel
	{
		public Guid PersonId { get; set; }
		public IList<Guid> SkillIdList { get; set; }
	}
}