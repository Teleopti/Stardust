﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public interface IScheduleLoader
	{
		IEnumerable<IVisualLayer> Load(Guid personId, DateOnly date);
	}
}