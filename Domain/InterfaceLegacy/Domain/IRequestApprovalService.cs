using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IRequestApprovalService
	{
		IEnumerable<IBusinessRuleResponse> Approve(IRequest request);
	}
}