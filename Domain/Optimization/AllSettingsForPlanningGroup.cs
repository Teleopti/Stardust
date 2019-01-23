using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AllSettingsForPlanningGroup : IEnumerable<PlanningGroupSettings>
	{
		private readonly IEnumerable<PlanningGroupSettings> _planningGroupSettings;
		public Percent PreferenceValue { get; private set; }
		public TeamSettings TeamSettings { get; }

		public AllSettingsForPlanningGroup(IEnumerable<PlanningGroupSettings> planningGroupSettings,
			Percent preferenceValue, TeamSettings teamSettings)
		{
			PreferenceValue = preferenceValue;
			TeamSettings = teamSettings;
			_planningGroupSettings = planningGroupSettings.OrderByDescending(x=>x.Priority).ToArray();
		}

		public PlanningGroupSettings ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _planningGroupSettings.First(x => x.IsValidForAgent(person, dateOnly));
		}

		public IDisposable DontUsePreferences()
		{
			var oldPreferenceValue = PreferenceValue;
			PreferenceValue = Percent.Zero;
			return new GenericDisposable(() => PreferenceValue = oldPreferenceValue);
		}

		public IEnumerator<PlanningGroupSettings> GetEnumerator()
		{
			return _planningGroupSettings.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}