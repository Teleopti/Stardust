using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class NotOvertimeRequestSpecification : Specification<IPersonRequest>
	{
		public override bool IsSatisfiedBy(IPersonRequest personRequest)
		{
			return !(personRequest.Request is IOvertimeRequest);
		}
	}
}
