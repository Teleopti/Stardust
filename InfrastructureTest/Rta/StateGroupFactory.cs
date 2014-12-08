using System;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public static class StateGroupFactory
	{
		public static RtaStateGroup CreateDefaultStateGroup(IBusinessUnit businessUnitId)
		{
			var stateGroup = new RtaStateGroup(Guid.NewGuid().ToString(), true, true);
			stateGroup.SetBusinessUnit(businessUnitId);
			return stateGroup;
		}
	}
}