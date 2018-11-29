using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestApprovalServiceFactory
	{
		IRequestApprovalService MakeAbsenceRequestApprovalService(IScheduleDictionary scheduleDictionary,
			IScenario scenario, IPerson person);

		IRequestApprovalService MakeShiftTradeRequestApprovalService(IScheduleDictionary scheduleDictionary, IPerson person);

		IRequestApprovalService MakeOvertimeRequestApprovalService(IDictionary<DateTimePeriod,IList<ISkill>> validatedSkillDictionary);
	}
}