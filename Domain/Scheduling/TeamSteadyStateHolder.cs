using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ITeamSteadyStateHolder
	{
		bool IsSteadyState(IGroupPerson groupPerson);
		void SetSteadyState(IGroupPerson groupPerson, bool state);
	}

	public class TeamSteadyStateHolder : ITeamSteadyStateHolder
	{
		private readonly IDictionary<Guid, bool> _teamSteadyStates;
 
		public TeamSteadyStateHolder(IDictionary<Guid, bool> teamSteadyStates)
		{
			_teamSteadyStates = teamSteadyStates;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool IsSteadyState(IGroupPerson groupPerson)	
		{
			if (!groupPerson.Id.HasValue) return false;
			if (!_teamSteadyStates.ContainsKey(groupPerson.Id.Value)) return false;

			return _teamSteadyStates[groupPerson.Id.Value];
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetSteadyState(IGroupPerson groupPerson, bool state)
		{
			if (!groupPerson.Id.HasValue) return;
			if (!_teamSteadyStates.ContainsKey(groupPerson.Id.Value))return;

			_teamSteadyStates.Remove(groupPerson.Id.Value);
			_teamSteadyStates.Add(groupPerson.Id.Value, false);
		}
	}
}
