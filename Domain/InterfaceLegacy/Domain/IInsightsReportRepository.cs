using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IInsightsReportRepository : IRepository<IInsightsReport>
	{
		IList<IInsightsReport> GetAllValidReports();

		void AddNewInsightsReport(Guid reportId, string reportName);
		void UpdateInsightsReport(Guid reportId, string reportName);
		void DeleteInsightsReport(Guid reportId);
	}
}