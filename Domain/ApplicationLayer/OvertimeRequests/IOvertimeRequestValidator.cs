using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public interface IOvertimeRequestValidator
	{
		OvertimeRequestValidationResult Validate(IPersonRequest personRequest);
	}
}