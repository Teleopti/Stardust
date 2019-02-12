using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Insights;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeInsightsReportRepository : IInsightsReportRepository
	{
		private readonly List<IInsightsReport> reports = new List<IInsightsReport>();

		public void Add(IInsightsReport root)
		{
			reports.Add(root);
		}

		public void Remove(IInsightsReport root)
		{
			reports.Remove(root);
		}

		public IInsightsReport Get(Guid id)
		{
			return reports.SingleOrDefault(x => x.Id == id);
		}

		public IInsightsReport Load(Guid id)
		{
			return reports.Single(x => x.Id == id);
		}

		public IEnumerable<IInsightsReport> LoadAll()
		{
			return reports;
		}

		public IList<IInsightsReport> GetAllValidReports()
		{
			return reports;
		}

		public void AddNewInsightsReport(Guid reportId, string reportName)
		{
			var currentUser = PersonFactory.CreatePersonWithId();
			var newReport = new InsightsReport
			{
				Name = reportName,
				CreatedBy = currentUser,
				CreatedOn = DateTime.Now,
				//UpdatedBy = currentUser,
				UpdatedOn = DateTime.Now,
			};
			newReport.SetId(reportId);
			reports.Add(newReport);
		}

		public void UpdateInsightsReport(Guid reportId, string reportName)
		{
			var report = Get(reportId);
			if (report == null) return;

			report.Name = reportName;
			//report.UpdatedBy = currentUser;
			//report.UpdatedOn = DateTime.Now;
		}

		public void DeleteInsightsReport(Guid reportId)
		{
			var report = reports.SingleOrDefault(x => x.Id == reportId);
			if (report != null) reports.Remove(report);
		}
	}
}