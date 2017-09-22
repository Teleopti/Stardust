using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportItemsProvider : IReportItemsProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly IReportUrl _matrixWebsiteUrl;

		public ReportItemsProvider(IReportsProvider reportsProvider, IReportUrl matrixWebsiteUrl)
		{
			_reportsProvider = reportsProvider;
			_matrixWebsiteUrl = matrixWebsiteUrl;
		}

		public List<ReportItem> GetReportItems()
		{
			var reports = _reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription);
			var reportItems = new List<ReportItem>();

			foreach (var applicationFunction in reports)
			{
				reportItems.Add(new ReportItem
				{
					Url = _matrixWebsiteUrl.Build(applicationFunction.ForeignId),
					Name = applicationFunction.LocalizedFunctionDescription,
				});
			}

			return reportItems;
		}
	}
}