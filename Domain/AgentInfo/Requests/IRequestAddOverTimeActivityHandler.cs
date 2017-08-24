using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public interface IRequestAddOverTimeActivityHandler
	{
		void Handle(Guid activityId, IOvertimeRequest overtimeRequest);
	}
}