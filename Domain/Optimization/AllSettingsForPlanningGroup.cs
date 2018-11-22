﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AllSettingsForPlanningGroup : IEnumerable<PlanningGroupSettings>
	{
		private readonly IEnumerable<PlanningGroupSettings> _planningGroupSettings;

		public AllSettingsForPlanningGroup(IEnumerable<PlanningGroupSettings> planningGroupSettings, Percent preferenceValue)
		{
			PreferenceValue = preferenceValue;
			_planningGroupSettings = planningGroupSettings.OrderByDescending(x=>x.Priority).ToArray();
		}
		
		public Percent PreferenceValue { get; set; }

		public PlanningGroupSettings ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _planningGroupSettings.First(x => x.IsValidForAgent(person, dateOnly));
		}

		public IDisposable ChangeSettingInThisScope(Percent newPreferenceValue)
		{
			var oldPreferenceValue = PreferenceValue;
			PreferenceValue = newPreferenceValue;
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