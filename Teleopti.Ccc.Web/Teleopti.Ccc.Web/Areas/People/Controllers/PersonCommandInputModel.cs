using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PersonCommandInputModel
	{
		public Guid PersonId { get; set; }
	}
	public class SkillUpdateCommandInputModel : PersonCommandInputModel
	{
		public IEnumerable<Guid> SkillIdList { get; set; }
	}
	public class ShiftBagUpdateCommandInputModel: PersonCommandInputModel
	{
		public Guid? ShiftBagId { get; set; }
	}
}