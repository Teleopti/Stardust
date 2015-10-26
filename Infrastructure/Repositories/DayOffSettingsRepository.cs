using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DayOffSettingsRepository : IDayOffSettingsRepository
	{
		public DayOffSettingsRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
		}

		public void Add(DayOffSettings root)
		{
			throw new NotImplementedException();
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
			return new[]
			{
				new DayOffSettings()
			};
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
	}
}