using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay
{
	public interface IScheduleLoader
	{
		IEnumerable<IVisualLayer> Load(Guid personId, DateOnly date);
	}
}