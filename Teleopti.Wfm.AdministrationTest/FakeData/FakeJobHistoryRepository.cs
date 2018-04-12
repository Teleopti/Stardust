using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobHistory;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakeJobHistoryRepository : IJobHistoryRepository
	{
		private readonly IDictionary<string, IList<BusinessUnitItem>> _businessUnitsDic = new ConcurrentDictionary<string, IList<BusinessUnitItem>>();
		private readonly IList<JobHistoryViewModel> _jobHistoryViewModels = new List<JobHistoryViewModel>();

		public void AddBusinessUnits(IList<BusinessUnitItem> businessUnits, string connectionString)
		{
			if (_businessUnitsDic.ContainsKey(connectionString))
				_businessUnitsDic.Remove(connectionString);
			_businessUnitsDic.Add(connectionString, businessUnits);
		}

		public IList<JobHistoryViewModel> GetEtlJobHistory(DateTime startDate, DateTime endDate, List<Guid> businessUnitIds, bool showOnlyErrors, string connectionString)
		{
			if (businessUnitIds.First() == new Guid("00000000-0000-0000-0000-000000000002"))
				return _jobHistoryViewModels.ToList();
			return _jobHistoryViewModels.Where(x => businessUnitIds.Select(y => y.ToString()).Contains(x.BusinessUnitName)).ToList();
		}

		public IList<BusinessUnitItem> GetBusinessUnitsIncludingAll(string connectionString)
		{
			if(!_businessUnitsDic.ContainsKey(connectionString))
				return new List<BusinessUnitItem>();
			return _businessUnitsDic[connectionString];
		}

		public void AddJobHistory(JobHistoryViewModel jobHistoryViewModel)
		{
			_jobHistoryViewModels.Add(jobHistoryViewModel);
		}
	}
}