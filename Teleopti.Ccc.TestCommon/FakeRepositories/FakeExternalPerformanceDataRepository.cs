using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalPerformanceDataRepository : IExternalPerformanceDataRepository
	{
		private readonly IList<IExternalPerformanceData> _performanceDataList;

		public FakeExternalPerformanceDataRepository()
		{
			_performanceDataList = new List<IExternalPerformanceData>();
		}

		public void Add(IExternalPerformanceData model)
		{
			_performanceDataList.Add(model);
		}

		public void Remove(IExternalPerformanceData model)
		{
			_performanceDataList.Remove(Get(model.Id.Value));
		}

		public IExternalPerformanceData Get(Guid id)
		{
			return _performanceDataList.FirstOrDefault(x => x.Id == id);
		}

		public IExternalPerformanceData Load(Guid id)
		{
			return Get(id);
		}

		public IList<IExternalPerformanceData> LoadAll()
		{
			return _performanceDataList;
		}
	}
}
