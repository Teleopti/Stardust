using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class WorkRuleSettingsRepository : IWorkRuleSettingsRepository
	{
		public WorkRuleSettingsRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
		}

		public void Add(WorkRuleSettings root)
		{
			throw new NotImplementedException();
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
			return new[]
			{
				new WorkRuleSettings()
			};
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