using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffRulesRepository : IDayOffRulesRepository
	{
		private readonly IList<DayOffRules> _workRuleSettings = new List<DayOffRules>();

		public void Add(DayOffRules root)
		{
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

		public void AddRange(IEnumerable<DayOffRules> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public DayOffRules Default()
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
	}
}