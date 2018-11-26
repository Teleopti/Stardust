using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroup : NonversionedAggregateRootWithBusinessUnit, IDeleteTag
	{
		private readonly ISet<IFilter> _filters = new HashSet<IFilter>();
		private readonly IList<PlanningGroupSettings> _settings = new List<PlanningGroupSettings>();
		private bool _isDeleted;
		private Percent _preferenceValue;

		public PlanningGroup()
		{
			Name = string.Empty;
			var planningGroupSettings = new PlanningGroupSettings();
			planningGroupSettings.SetAsDefault();
			addPlanningGroupSetting(planningGroupSettings);
			_preferenceValue = new Percent(0.8);
		}

		public virtual IEnumerable<IFilter> Filters => _filters;
		public virtual string Name { get; set; }
		public virtual AllSettingsForPlanningGroup Settings => new AllSettingsForPlanningGroup(_settings, _preferenceValue);

		public virtual void SetGlobalValues(Percent preferenceValue)
		{
			_preferenceValue = preferenceValue;
		}

		public virtual void ClearFilters()
		{
			_filters.Clear();
		}

		public virtual PlanningGroup AddFilter(IFilter filter)
		{
			_filters.Add(filter);
			return this;
		}

		public virtual bool IsDeleted => _isDeleted;
		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual void AddSetting(PlanningGroupSettings planningGroupSettings)
		{
			if(planningGroupSettings.Default)
				throw new ArgumentException("Cannot add default planning group settings.");
			addPlanningGroupSetting(planningGroupSettings);
		}
		
		private void addPlanningGroupSetting(PlanningGroupSettings planningGroupSettings)
		{
			planningGroupSettings.SetParent(this);
			_settings.Add(planningGroupSettings);
		}

		public virtual void RemoveSetting(PlanningGroupSettings planningGroupSettings)
		{
			if(planningGroupSettings.Default)
				throw new ArgumentException("Cannot remove default PlanningGroupSettings.");
			_settings.Remove(planningGroupSettings);
		}
		
		
		public virtual void ModifyDefault(Action<PlanningGroupSettings> action)
		{
			var currDefault = Settings.Single(x => x.Default);
			action(currDefault);
		}
	}
}