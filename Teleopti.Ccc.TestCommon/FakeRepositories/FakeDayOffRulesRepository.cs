﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffRulesRepository : IDayOffRulesRepository
	{
		private readonly List<DayOffRules> _workRuleSettings = new List<DayOffRules>();

		public void Add(DayOffRules root)
		{
			if (root.Default && root.PlanningGroup == null)
			{
				var currDefault = Default();
				if (currDefault != null)
					_workRuleSettings.Remove(currDefault);
			}
			_workRuleSettings.Add(root);
		}

		public void Remove(DayOffRules root)
		{
			_workRuleSettings.Remove(root);
		}

		public DayOffRules Get(Guid id)
		{
			return _workRuleSettings.SingleOrDefault(x => x.Id == id);
		}

		public IList<DayOffRules> LoadAll()
		{
			return _workRuleSettings.ToArray();
		}

		public DayOffRules Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		private DayOffRules Default()
		{
			var curr = _workRuleSettings.SingleOrDefault(x => x.Default);
			if (curr != null)
				return curr;
			return DayOffRules.CreateDefault();
		}

		public void HasDefault(Action<DayOffRules> actionOnDefaultInstance)
		{
			var defaultSettings = DayOffRules.CreateDefault();
			actionOnDefaultInstance(defaultSettings);
			_workRuleSettings.Add(defaultSettings);
		}

		public IList<DayOffRules> LoadAllByPlanningGroup(IPlanningGroup planningGroup)
		{
			return _workRuleSettings.Where(x => x.PlanningGroup == planningGroup).ToList();
		}

		public IList<DayOffRules> LoadAllWithoutPlanningGroup()
		{
			return _workRuleSettings.Where(x => x.PlanningGroup == null).ToList();
		}

		public void RemoveForPlanningGroup(IPlanningGroup planningGroup)
		{
			_workRuleSettings.RemoveAll(dayOffRule => dayOffRule.PlanningGroup == planningGroup);
		}
	}
}