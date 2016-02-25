using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsPersonPeriod
	{
		int PersonId { get; set; }
		Guid PersonCode { get; set; }
	    DateTime ValidFromDate { get; set; }
	    DateTime ValidToDate { get; set; }
        int ValidFromDateId { get; set; }
        int ValidFromIntervalId { get; set; }
        int ValidToDateId { get; set; }
        int ValidToIntervalId { get; set; }
        Guid PersonPeriodCode { get; set; }
        string PersonName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string EmploymentNumber { get; set; }
        int EmploymentTypeCode { get; set; }
        string EmploymentTypeName { get; set; }
        Guid ContractCode { get; set; }
        string ContractName { get; set; }
        Guid ParttimeCode { get; set; }
        string ParttimePercentage { get; set; }
        int TeamId { get; set; }
        Guid TeamCode { get; set; }
        string TeamName { get; set; }
        int SiteId { get; set; }
        Guid SiteCode { get; set; }
        string SiteName { get; set; }
        int BusinessUnitId { get; set; }
        Guid BusinessUnitCode { get; set; }
        string BusinessUnitName { get; set; }
        int SkillsetId { get; set; }
        string Email { get; set; }
        string Note { get; set; }
        DateTime EmploymentStartDate { get; set; }
        DateTime EmploymentEndDate { get; set; }
        int TimeZoneId { get; set; }
        bool IsAgent { get; set; }
        bool IsUser { get; set; }
        int DatasourceId { get; set; }
        DateTime InsertDate { get; set; }
        DateTime UpdateDate { get; set; }
        DateTime DatasourceUpdateDate { get; set; }
        bool ToBeDeleted { get; set; }
        string WindowsDomain { get; set; }
        string WindowsUsername { get; set; }
	    int ValidToDateIdMaxDate { get; set; }
	    int ValidToIntervalIdMaxDate { get; set; }
	    int ValidFromDateIdLocal { get; set; }
	    int ValidToDateIdLocal { get; set; }
	    DateTime ValidFromDateLocal { get; set; }
	    DateTime ValidToDateLocal { get; set; }

	}
}
