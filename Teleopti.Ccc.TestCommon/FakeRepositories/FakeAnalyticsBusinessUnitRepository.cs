using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsBusinessUnitRepository : IAnalyticsBusinessUnitRepository
	{
		public bool ReturnNull { get; set; }
		public bool UseList { get; set; } // So we can gradually change the behavior of this fake repo
		private readonly List<AnalyticBusinessUnit> _businessUnits = new List<AnalyticBusinessUnit>();

		public AnalyticBusinessUnit Get(Guid businessUnitCode)
		{
			if (UseList) return _businessUnits.FirstOrDefault(x => x.BusinessUnitCode == businessUnitCode);
			return ReturnNull ? null : new AnalyticBusinessUnit {BusinessUnitId = 1, DatasourceId = 1};
		}

		public void AddOrUpdate(AnalyticBusinessUnit businessUnit)
		{
			_businessUnits.RemoveAll(x => x.BusinessUnitCode == businessUnit.BusinessUnitCode);
			if (businessUnit.BusinessUnitId == 0)
				businessUnit.BusinessUnitId = _businessUnits.Select(a => a.BusinessUnitId).DefaultIfEmpty(0).Max() + 1;
			businessUnit.DatasourceId = 1;
			_businessUnits.Add(businessUnit);
		}
	}
}