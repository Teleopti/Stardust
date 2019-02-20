using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Insights;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class InsightsReportRepository : Repository<IInsightsReport>, IInsightsReportRepository
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public InsightsReportRepository(ICurrentUnitOfWork currentUnitOfWork, ILoggedOnUser loggedOnUser)
			: base(currentUnitOfWork, null, null)
		{
			_loggedOnUser = loggedOnUser;
		}

		public IList<IInsightsReport> GetAllValidReports()
		{
			return Session.CreateCriteria(typeof(InsightsReport), "rep")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IInsightsReport>();
		}

		public void AddNewInsightsReport(Guid reportId, string reportName)
		{
			var newReport = new InsightsReport
			{
				Name = reportName,
				CreatedBy = _loggedOnUser.CurrentUser(),
				CreatedOn = DateTime.UtcNow
			};
			newReport.SetId(reportId);
			newReport.SetVersion(0);
			Session.Save(newReport);
		}

		public void UpdateInsightsReport(Guid reportId, string reportName)
		{
			var report = base.Get(reportId);
			if (report == null)
			{
				AddNewInsightsReport(reportId, reportName);
				return;
			}

			report.Name = reportName;
			report.SetVersion(report.Version.GetValueOrDefault() + 1);
			Session.Update(report);
		}

		public void DeleteInsightsReport(Guid reportId)
		{
			var report = base.Get(reportId);
			if (report != null)
			{
				base.Remove(report);
			}
		}
	}
}
