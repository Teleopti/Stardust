using System.Collections.Generic;
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
		private readonly IMatrixWebsiteUrl _matrixWebsiteUrl;

		public ReportItemsProvider(IReportsProvider reportsProvider, ICurrentBusinessUnit currentBusinessUnit, IMatrixWebsiteUrl matrixWebsiteUrl)
		{
			_reportsProvider = reportsProvider;
			_currentBusinessUnit = currentBusinessUnit;
			_matrixWebsiteUrl = matrixWebsiteUrl;
		}

		public List<ReportItem> GetReportItems()
		{
			var realBu = _currentBusinessUnit.Current().Id;
			var matrixWebsiteUrl = _matrixWebsiteUrl.Build();

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