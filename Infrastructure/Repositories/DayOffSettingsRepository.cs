using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;
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
			var ret =
				new DayOffSettings
				{
					//TODO: put default values here until we use real db values. remove this later...
					DayOffsPerWeek = new MinMax<int>(1, 3),
					ConsecutiveWorkdays = new MinMax<int>(2, 6),
					ConsecutiveDayOffs = new MinMax<int>(1, 3),
					Default = true
				};
			ret.SetId(Guid.NewGuid());
			return new[] {ret};
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