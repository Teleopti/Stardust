using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IApprovePersonRequestCommand : IHandlePersonRequestCommand
	{
		IList<IBusinessRuleResponse> Approve(INewBusinessRuleCollection newBusinessRules);
	}
}