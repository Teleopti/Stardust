using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePlanningGroupSettingsRepository : IPlanningGroupSettingsRepository
	{
		private readonly List<PlanningGroupSettings> _workRuleSettings = new List<PlanningGroupSettings>();

		public void Add(PlanningGroupSettings root)
		{
			if (root.Default && root.PlanningGroup == null)
			{
				var currDefault = Default();
				if (currDefault != null)
					_workRuleSettings.Remove(currDefault);
			}
			_workRuleSettings.Add(root);
		}

		public void Remove(PlanningGroupSettings root)
		{
			_workRuleSettings.Remove(root);
		}

		public PlanningGroupSettings Get(Guid id)
		{
			return _workRuleSettings.SingleOrDefault(x => x.Id == id);
		}

		public IEnumerable<PlanningGroupSettings> LoadAll()
		{
			return _workRuleSettings.ToArray();
		}

		public PlanningGroupSettings Load(Guid id)
		{
			throw new NotImplementedException();
		}

		private PlanningGroupSettings Default()
		{
			var curr = _workRuleSettings.SingleOrDefault(x => x.Default);
			if (curr != null)
				return curr;
			return PlanningGroupSettings.CreateDefault();
		}

		public void HasDefault(Action<PlanningGroupSettings> actionOnDefaultInstance, IPlanningGroup planningGroup)
		{
			var defaultSettings = PlanningGroupSettings.CreateDefault(planningGroup);
			actionOnDefaultInstance(defaultSettings);
			_workRuleSettings.Add(defaultSettings);
		}

		public AllPlanningGroupSettings LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return new AllPlanningGroupSettings(_workRuleSettings.Where(x => x.PlanningGroup == planningGroup).OrderBy(x => x.Default).ThenByDescending(x => x.Priority));
		}
		
		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			_workRuleSettings.RemoveAll(dayOffRule => dayOffRule.PlanningGroup == planningGroup);
		}
	}
}