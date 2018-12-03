using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailableSkillsValidationResult : OvertimeRequestValidationResult
	{
		public IDictionary<DateTimePeriod,IList<ISkill>> SkillDictionary { get; set; }
	}
}