using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestContractWorkRulesValidator
	{
		OvertimeRequestValidationResult Validate(IPersonRequest personRequest, OvertimeRequestSkillTypeFlatOpenPeriod overtimeRequestOpenPeriod);
	}
}