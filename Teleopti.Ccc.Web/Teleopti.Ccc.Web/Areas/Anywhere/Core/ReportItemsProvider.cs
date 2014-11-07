using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class ReportItemsProvider : IReportItemsProvider
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public ReportItemsProvider(IReportsProvider reportsProvider, ICurrentBusinessUnit currentBusinessUnit)
		{
			_reportsProvider = reportsProvider;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public List<ReportItem> GetReportItems()
		{
			var realBu = _currentBusinessUnit.Current().Id;
			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/")) matrixWebsiteUrl += "/";

			var reports = new List<IApplicationFunction>(_reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription));
			var reportItems = new List<ReportItem>();
			foreach (var applicationFunction in reports)
			{
				var url = string.Format(CultureInfo.CurrentCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
					matrixWebsiteUrl, applicationFunction.ForeignId, realBu);

				reportItems.Add(new ReportItem
				{
					Url = url,
					Name = applicationFunction.LocalizedFunctionDescription,
				});
			}

			return reportItems;
		}
	}
}