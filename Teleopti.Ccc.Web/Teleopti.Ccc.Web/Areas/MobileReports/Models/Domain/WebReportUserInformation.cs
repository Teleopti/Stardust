namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain
{
	using System;

	public class WebReportUserInformation
	{
		public string TimeZoneCode { get; set; }

		public Guid BusinessUnitCode { get; set; }

		public int LanguageId { get; set; }

		public Guid PersonCode { get; set; }
	}
}