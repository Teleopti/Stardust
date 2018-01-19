using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestAvailableSkillsValidator
	{
		OvertimeRequestAvailableSkillsValidationResult Validate(IPersonRequest personRequest, IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod);
	}
}