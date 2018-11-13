using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.Infrastructure.AgentState
{
	public class AgentStateReadModelForTest : AgentStateReadModel
	{
		public AgentStateReadModelForTest()
		{
			ReceivedTime = DateTime.UtcNow;
			IsRuleAlarm = true;
			BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value;
		}
	}

	public class AssociationInfoForTest : AssociationInfo
	{
		public AssociationInfoForTest()
		{
			BusinessUnitId = ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.Value;
		}
	}
}