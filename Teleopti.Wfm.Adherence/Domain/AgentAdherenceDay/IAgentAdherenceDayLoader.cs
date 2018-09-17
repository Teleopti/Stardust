﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay
{
	public interface IAgentAdherenceDayLoader
	{
		IAgentAdherenceDay LoadUntilNow(Guid personId, DateOnly date);
		IAgentAdherenceDay Load(Guid personId, DateOnly date);
	}
}