using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobHistory;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeJobHistoryRepository : IJobHistoryRepository
	{
		private readonly IDictionary<string, IList<BusinessUnitItem>> _businessUnitsDic = new ConcurrentDictionary<string, IList<BusinessUnitItem>>();

		public void AddBusinessUnits(IList<BusinessUnitItem> businessUnits, string connectionString)
		{
			if (_businessUnitsDic.ContainsKey(connectionString))
				_businessUnitsDic.Remove(connectionString);
			_businessUnitsDic.Add(connectionString, businessUnits);
		}

		public IList<JobHistoryViewModel> GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors, string connectionString)
		{
			throw new NotImplementedException();
		}

		public IList<BusinessUnitItem> GetBusinessUnitsIncludingAll(string connectionString)
		{
			return _businessUnitsDic[connectionString];
		}
	}
}