using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalPerformanceDataReadModelRepository : IExternalPerformanceDataReadModelRepository
	{
		private readonly IList<ExternalPerformanceData> _performanceDataList;

		public FakeExternalPerformanceDataReadModelRepository()
		{
			_performanceDataList = new List<ExternalPerformanceData>();
		}

		public void Add(ExternalPerformanceData model)
		{
			_performanceDataList.Add(model);
		}

		public IList<ExternalPerformanceData> GetAll()
		{
			return _performanceDataList;
		}
	}
}
