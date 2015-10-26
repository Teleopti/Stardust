using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeDayOffSettingsRepository : IDayOffSettingsRepository
	{
		private readonly IList<DayOffSettings> _workRuleSettings = new List<DayOffSettings>();

		public FakeDayOffSettingsRepository()
		{
			//adding default settings
			_workRuleSettings.Add(new DayOffSettings().WithId());
		}

		public void Add(DayOffSettings root)
		{
			_workRuleSettings.Add(root);
		}

		public void Remove(DayOffSettings root)
		{
			throw new NotImplementedException();
		}

		public DayOffSettings Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<DayOffSettings> LoadAll()
		{
			return _workRuleSettings.ToArray();
		}

		public DayOffSettings Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<DayOffSettings> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }

		public DayOffSettings DefaultSettings()
		{
			return _workRuleSettings.Single(x => x.Default);
		}
	}
}