using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;
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
			throw new NotImplementedException();
		}

		public DayOffRules Get(Guid id)
		{
			throw new NotImplementedException();
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
			return new DayOffRules
			{
				//should be same as default values in db script
				DayOffsPerWeek = new MinMax<int>(1, 3),
				ConsecutiveWorkdays = new MinMax<int>(2, 6),
				ConsecutiveDayOffs = new MinMax<int>(1, 3)
			}.MakeDefault_UseOnlyFromTest();
		}

		public void HasDefault(Action<DayOffRules> actionOnDefaultInstance)
		{
			var defaultSettings = new DayOffRules
			{
				//should be same as default values in db script
				DayOffsPerWeek = new MinMax<int>(1, 3),
				ConsecutiveWorkdays = new MinMax<int>(2, 6),
				ConsecutiveDayOffs = new MinMax<int>(1, 3)
			}.MakeDefault_UseOnlyFromTest();
			actionOnDefaultInstance(defaultSettings);
			_workRuleSettings.Add(defaultSettings);
		}
	}
}