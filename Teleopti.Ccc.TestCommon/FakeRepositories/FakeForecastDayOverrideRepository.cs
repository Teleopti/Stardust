using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeForecastDayOverrideRepository : IForecastDayOverrideRepository
	{
		private readonly IFakeStorage _storage;

		public FakeForecastDayOverrideRepository(IFakeStorage storage)
		{
			_storage = storage;
		}

		public ICollection<IForecastDayOverride> FindRange(DateOnlyPeriod period, IWorkload workload, IScenario scenario)
		{
			return _storage.LoadAll<IForecastDayOverride>()
				.Where(x => x.Workload.Equals(workload) && x.Scenario.Equals(scenario) && period.Contains(x.Date))
				.ToArray();
		}

		public void Add(IForecastDayOverride root)
		{
			_storage.Add(root);
		}

		public void Remove(IForecastDayOverride root)
		{
			_storage.Remove(root);
		}

		public IForecastDayOverride Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IForecastDayOverride Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IForecastDayOverride> LoadAll()
		{
			throw new NotImplementedException();
		}
	}
}