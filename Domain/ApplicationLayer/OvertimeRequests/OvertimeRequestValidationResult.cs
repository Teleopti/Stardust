using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestValidationResult
	{
		public bool IsValid { get; set; }

		public bool ShouldDenyIfInValid { get; set; } = true;

		public string[] InvalidReasons { get; set; }
		public BusinessRuleFlags BrokenBusinessRules { get; set; }
	}
}