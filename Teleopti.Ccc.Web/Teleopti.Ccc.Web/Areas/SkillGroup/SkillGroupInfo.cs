using System.Collections.Generic;
using Teleopti.Ccc.Domain.SkillGroupManagement;

namespace Teleopti.Ccc.Web.Areas.SkillGroup
{
	public class SkillGroupInfo
	{
		public bool HasPermissionToModifySkillArea { get; set; }
		public IEnumerable<SkillGroupViewModel> SkillAreas { get; set; }
	}
}