using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeWorkRuleSettingsRepository : IWorkRuleSettingsRepository
	{
		private readonly IList<WorkRuleSettings> _workRuleSettings = new List<WorkRuleSettings>();

		public void Add(WorkRuleSettings root)
		{
			_workRuleSettings.Add(root);
		}

		public void Remove(WorkRuleSettings root)
		{
			throw new NotImplementedException();
		}

		public WorkRuleSettings Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<WorkRuleSettings> LoadAll()
		{
			return _workRuleSettings.ToArray();
		}

		public WorkRuleSettings Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<WorkRuleSettings> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
	}
}