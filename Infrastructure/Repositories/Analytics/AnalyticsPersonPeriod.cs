using System;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPersonPeriod : IAnalyticsPersonPeriod
	{
		public int PersonId { get; set; }
		public Guid PersonCode { get; set; }
		public DateTime ValidFromDate { get; set; }
		public DateTime ValidToDate { get; set; }
		public int ValidFromDateId { get; set; }
		public int ValidFromIntervalId { get; set; }
		public int ValidToDateId { get; set; }
		public int ValidToIntervalId { get; set; }
		public Guid PersonPeriodCode { get; set; }
		public string PersonName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public int EmploymentTypeCode { get; set; }
		public string EmploymentTypeName { get; set; }
		public Guid ContractCode { get; set; }
		public string ContractName { get; set; }
		public Guid ParttimeCode { get; set; }
		public string ParttimePercentage { get; set; }
		public int TeamId { get; set; }
		public Guid TeamCode { get; set; }
		public string TeamName { get; set; }
		public int SiteId { get; set; }
		public Guid SiteCode { get; set; }
		public string SiteName { get; set; }
		public int BusinessUnitId { get; set; }
		public Guid BusinessUnitCode { get; set; }
		public string BusinessUnitName { get; set; }
		public int SkillsetId { get; set; }
		public string Email { get; set; }
		public string Note { get; set; }
		public DateTime EmploymentStartDate { get; set; }
		public DateTime EmploymentEndDate { get; set; }
		public int TimeZoneId { get; set; }
		public bool IsAgent { get; set; }
		public bool IsUser { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public bool ToBeDeleted { get; set; }
		public string WindowsDomain { get; set; }
		public string WindowsUsername { get; set; }
		public int ValidToDateIdMaxDate { get; set; }
		public int ValidToIntervalIdMaxDate { get; set; }
		public int ValidFromDateIdLocal { get; set; }
		public int ValidToDateIdLocal { get; set; }
		public DateTime ValidFromDateLocal { get; set; }
		public DateTime ValidToDateLocal { get; set; }
	}
}