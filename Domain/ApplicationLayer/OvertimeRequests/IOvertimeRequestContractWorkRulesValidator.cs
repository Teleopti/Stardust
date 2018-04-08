using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestContractWorkRulesValidator
	{
		OvertimeRequestValidationResult Validate(OvertimeRequestValidationContext context, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);
	}
}