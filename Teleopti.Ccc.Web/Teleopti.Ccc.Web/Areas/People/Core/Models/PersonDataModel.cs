using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class PersonDataModel
	{
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Team { get; set; }
		public IList<Guid> SkillIdList { get; set; }
		public Guid? ShiftBagId { get; set; }
	}
}