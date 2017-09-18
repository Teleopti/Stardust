using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestAvailableSkillsValidationResult : OvertimeRequestValidationResult
	{
		public ISkill[] Skills { get; set; }
	}
}