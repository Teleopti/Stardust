using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffRulesRepository : IDayOffRulesRepository
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

		public IList<PlanningGroupSettings> LoadAll()
		{
			return _workRuleSettings.ToArray();
		}

		public PlanningGroupSettings Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		private PlanningGroupSettings Default()
		{
			var curr = _workRuleSettings.SingleOrDefault(x => x.Default);
			if (curr != null)
				return curr;
			return PlanningGroupSettings.CreateDefault();
		}

		public void HasDefault(Action<PlanningGroupSettings> actionOnDefaultInstance)
		{
			var defaultSettings = PlanningGroupSettings.CreateDefault();
			actionOnDefaultInstance(defaultSettings);
			_workRuleSettings.Add(defaultSettings);
		}

		public IList<PlanningGroupSettings> LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return _workRuleSettings.Where(x => x.PlanningGroup == planningGroup).ToList();
		}

		public IList<PlanningGroupSettings> LoadAllWithoutPlanningGroup()
		{
			return _workRuleSettings.Where(x => x.PlanningGroup == null).ToList();
		}

		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			_workRuleSettings.RemoveAll(dayOffRule => dayOffRule.PlanningGroup == planningGroup);
		}
	}
}