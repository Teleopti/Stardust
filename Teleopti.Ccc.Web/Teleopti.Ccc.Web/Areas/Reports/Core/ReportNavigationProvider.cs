using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Areas.Reports.Models;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class ReportNavigationProvider : IReportNavigationProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _reportUrl;

		public ReportNavigationProvider(IReportsProvider reportsProvider, IReportUrl reportUrl)
		{
			_reportsProvider = reportsProvider;
			_reportUrl = reportUrl;
		}

		public IList<ReportItem> GetNavigationItems()
		{
			var grantedFuncs = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);

			return grantedFuncs.Select(applicationFunction => new ReportItem
			{
				Url = _reportUrl.Build(applicationFunction.ForeignId),
				Name = applicationFunction.LocalizedFunctionDescription,
			}).ToList();
		}
	}
}