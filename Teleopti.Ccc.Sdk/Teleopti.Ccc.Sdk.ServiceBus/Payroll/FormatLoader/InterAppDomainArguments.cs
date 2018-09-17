using System;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader
{
	[Serializable]
	public class InterAppDomainArguments
	{
		public string PayrollExportDto { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string DataSource { get; set; }
		public string  UserName { get; set; }
		public ISdkServiceFactory SdkServiceFactory { get; set; }
		public Guid PayrollResultId { get; set; }
		public string TenantName { get; set; }
		public string PayrollBasePath { get; set; }
	}

	public class InterAppDomainParameters
	{
		public static string PayrollResultParameter => "PayrollResult";
		public static string PayrollResultDetailsParameter => "PayrollResultDetails";
		public static string AppDomainArgumentsParameter => "AppDomainArguments";
		public static string PayrollFormatDtosParameter => "PayrollFormatDtos";
	}
}