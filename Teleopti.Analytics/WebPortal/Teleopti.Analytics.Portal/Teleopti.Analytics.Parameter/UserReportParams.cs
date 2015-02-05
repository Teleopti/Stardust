using System;

namespace Teleopti.Analytics.Parameters
{
	public class UserReportParams
	{
		public Guid BusinessUnitCode { get; set; }
		public int LangId { get; set; }
		public Guid CurrentUserGuid { get; set; }
		public string ConnectionString { get; set; }
	}
}