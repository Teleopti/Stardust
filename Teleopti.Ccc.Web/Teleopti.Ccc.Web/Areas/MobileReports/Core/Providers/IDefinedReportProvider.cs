namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Providers
{
	using System.Collections.Generic;

	using Models.Domain;

	public interface IDefinedReportProvider
	{
		IEnumerable<DefinedReportInformation> GetDefinedReports();
		IDefinedReport Get(string reportId);
	}
}