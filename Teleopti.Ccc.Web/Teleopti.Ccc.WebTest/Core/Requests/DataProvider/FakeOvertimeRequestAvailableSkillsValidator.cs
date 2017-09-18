using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	public class FakeOvertimeRequestAvailableSkillsValidator : IOvertimeRequestAvailableSkillsValidator
	{
		public OvertimeRequestAvailableSkillsValidationResult Validate(IPersonRequest personRequest)
		{
			return new OvertimeRequestAvailableSkillsValidationResult {IsValid = true};
		}
	}
}
