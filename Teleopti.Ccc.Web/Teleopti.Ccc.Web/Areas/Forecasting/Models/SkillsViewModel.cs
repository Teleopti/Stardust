using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class SkillsViewModel
	{
		public IEnumerable<SkillAccuracy> Skills { get; set; }
		public bool IsPermittedToModifySkill { get; set; }
	}
}