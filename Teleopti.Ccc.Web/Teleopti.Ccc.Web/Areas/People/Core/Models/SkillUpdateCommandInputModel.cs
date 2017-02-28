using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Controllers;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class SkillUpdateCommandInputModel : PersonCommandInputModel
	{
		public IEnumerable<Guid> SkillIdList { get; set; }
	}
}