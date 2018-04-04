using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailableSkillsValidationResult : OvertimeRequestValidationResult
	{
		public IDictionary<DateTimePeriod,IList<ISkill>> SkillDictionary { get; set; }
	}
}