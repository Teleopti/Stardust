using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
    public class AnalyticsPersonRepository : IAnalyticsPersonPeriodRepository
    {
        public int BusinessUnitId(Guid businessUnitCode)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(@"select business_unit_id from mart.dim_business_unit WITH (NOLOCK) WHERE business_unit_code=:businessUnitCode")
                    .SetGuid("businessUnitCode", businessUnitCode)
                    .UniqueResult<int>();
            }
        }

        public IList<IAnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(
                    @"SELECT person_id PersonId
                        , person_code PersonCode
                        , valid_from_date ValidFromDate
                        , valid_to_date ValidToDate
                        , valid_from_date_id ValidFromDateId
                        , valid_to_date_id ValidToDateId
                        , valid_to_interval_id ValidToIntervalId
                        , person_period_code PersonPeriodCode
                        , person_name PersonName
                        , first_name FirstName
                        , last_name LastName
                        , employment_number EmploymentNumber
                        , employment_type_code EmploymentTypeCode
                        , employment_type_name EmploymentTypeName
                        , contract_code ContractCode
                        , contract_name ContractName
                        , parttime_code ParttimeCode
                        , parttime_percentage ParttimePercentage
                        , team_id TeamId
                        , team_code TeamCode
                        , team_name TeamName
                        , site_id SiteId
                        , site_code SiteCode
                        , site_name SiteName
                        , business_unit_id BusinessUnitId
                        , business_unit_code BusinessUnitCode
                        , business_unit_name BusinessUnitName
                        , skillset_id SkillsetId
                        , email Email
                        , note Note
                        , employment_start_date EmploymentStartDate
                        , employment_end_date EmploymentEndDate
                        , time_zone_id TimeZoneId
                        , is_agent IsAgent
                        , is_user IsUser
                        , datasource_id DatasourceId
                        , insert_date InsertDate
                        , update_date UpdateDate
                        , datasource_update_date DatasourceUpdateDate
                        , to_be_deleted ToBeDeleted
                        , windows_domain WindowsDomain
                        , windows_username WindowsUsername from mart.dim_person WITH (NOLOCK) WHERE person_code =:code ")
                    .SetGuid("code", personCode)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsPersonPeriod)))
                    .SetReadOnly(true)
                    //.SetTimeout(120)
                    .List<IAnalyticsPersonPeriod>();
            }
        }

        public int SiteId(Guid siteCode, string siteName, int businessUnitId)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(@"mart.etl_dim_site_id_get @site_code=:siteCode , @site_name=:siteName, @business_unit_id=:businessUnitId")
                    .SetGuid("siteCode", siteCode)
                    .SetString("siteName", siteName)
                    .SetInt32("businessUnitId", businessUnitId)
                    .UniqueResult<int>();
            }
        }

        public int TeamId(Guid teamCode, int siteId, string teamName, int businessUnitId)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(@"mart.etl_dim_team_id_get @team_code=:teamCode,@team_name=:teamName, @site_id=:siteId , @business_unit_id=:businessUnitId")
                    .SetGuid("teamCode", teamCode)
                    .SetString("teamName", teamName)
                    .SetInt32("siteId", siteId)
                    .SetInt32("businessUnitId", businessUnitId)
                    .UniqueResult<int>();
            }
        }

        public int SkillSetId(IList<IAnalyticsSkill> skills)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                var skillSetCode = string.Join(",", skills.Select(a => a.SkillId).OrderBy(a => a));
                return uow.Session().CreateSQLQuery(
                    @"select skillset_id
                      from mart.dim_skillset WITH (NOLOCK) 
                      where skillset_code=:skillsetCode")
                    .SetString("skillsetCode", skillSetCode)
                    .UniqueResult<int>();
            }
        }

        public IList<IAnalyticsSkill> Skills(int businessUnitId)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                return uow.Session().CreateSQLQuery(
                    @"select 
	                    skill_id SkillId, 
	                    skill_code SkillCode,
	                    skill_name SkillName,
	                    time_zone_id TimeZoneId,
	                    forecast_method_code ForecastMethodCode,
	                    forecast_method_name ForecastMethodName,
	                    business_unit_id BusinessUnitId,
	                    datasource_id DatasourceId,
	                    insert_date InsertDate,
	                    update_date UpdateDate,
	                    datasource_update_date DatasourceUpdateDate,
	                    is_deleted IsDeleted
                    from mart.dim_skill WITH (NOLOCK)")
                    .SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsSkill)))
                    .SetReadOnly(true)
                    .List<IAnalyticsSkill>();
            }
        }

        public void AddPersonPeriod(IAnalyticsPersonPeriod personPeriod)
        {
            using (IStatelessUnitOfWork uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
            {
                uow.Session().CreateSQLQuery(
                    @"exec mart.[etl_dim_person_insert]
                     @person_code=:PersonCode
                    ,@valid_from_date=:ValidFromDate
                    ,@valid_to_date=:ValidToDate
                    ,@valid_from_date_id=:ValidFromDateId
                    ,@valid_from_interval_id=:ValidFromIntervalId
                    ,@valid_to_date_id=:ValidToDateId
                    ,@valid_to_interval_id=:ValidToIntervalId
                    ,@person_period_code=:PersonPeriodCode
                    ,@person_name=:PersonName
                    ,@first_name=:FirstName
                    ,@last_name=:LastName
                    ,@employment_number=:EmploymentNumber
                    ,@employment_type_code=:EmploymentTypeCode
                    ,@employment_type_name=:EmploymentTypeName
                    ,@contract_code=:ContractCode
                    ,@contract_name=:ContractName
                    ,@parttime_code=:ParttimeCode
                    ,@parttime_percentage=:ParttimePercentage
                    ,@team_id=:TeamId
                    ,@team_code=:TeamCode
                    ,@team_name=:TeamName
                    ,@site_id=:SiteId
                    ,@site_code=:SiteCode
                    ,@site_name=:SiteName
                    ,@business_unit_id=:BusinessUnitId
                    ,@business_unit_code=:BusinessUnitCode
                    ,@business_unit_name=:BusinessUnitName
                    ,@skillset_id=:SkillsetId
                    ,@email=:Email
                    ,@note=:Note
                    ,@employment_start_date=:EmploymentStartDate
                    ,@employment_end_date=:EmploymentEndDate
                    ,@time_zone_id=:TimeZoneId
                    ,@is_agent=:IsAgent
                    ,@is_user=:IsUser
                    ,@datasource_id=:DatasourceId
                    ,@insert_date=:InsertDate
                    ,@update_date=:UpdateDate
                    ,@datasource_update_date=:DatasourceUpdateDate
                    ,@to_be_deleted=:ToBeDeleted
                    ,@windows_domain=:WindowsDomain
                    ,@windows_username=:WindowsUsername
                    ,@valid_to_date_id_maxDate=:ValidToDateIdMaxDate
                    ,@valid_to_interval_id_maxDate=:ValidToIntervalIdMaxDate
                    ,@valid_from_date_id_local=:ValidFromDateIdLocal
                    ,@valid_to_date_id_local=:ValidToDateIdLocal
                    ,@valid_from_date_local=:ValidFromDateLocal
                    ,@valid_to_date_local=:ValidToDateLocal")
            .SetGuid("PersonCode", personPeriod.PersonCode)
            .SetDateTime("ValidFromDate", personPeriod.ValidFromDate)
            .SetDateTime("ValidToDate", personPeriod.ValidToDate)
            .SetInt32("ValidFromDateId", personPeriod.ValidFromDateId)
            .SetInt32("ValidFromIntervalId", personPeriod.ValidFromIntervalId)
            .SetInt32("ValidToDateId", personPeriod.ValidToDateId)
            .SetInt32("ValidToIntervalId", personPeriod.ValidToIntervalId)
            .SetGuid("PersonPeriodCode", personPeriod.PersonPeriodCode)
            .SetString("PersonName", personPeriod.PersonName)
            .SetString("FirstName", personPeriod.FirstName)
            .SetString("LastName", personPeriod.LastName)
            .SetString("EmploymentNumber", personPeriod.EmploymentNumber)
            .SetInt32("EmploymentTypeCode", personPeriod.EmploymentTypeCode)
            .SetString("EmploymentTypeName", personPeriod.EmploymentTypeName)
            .SetGuid("ContractCode", personPeriod.ContractCode)
            .SetString("ContractName", personPeriod.ContractName)
            .SetGuid("ParttimeCode", personPeriod.ParttimeCode)
            .SetString("ParttimePercentage", personPeriod.ParttimePercentage)
            .SetInt32("TeamId", personPeriod.TeamId)
            .SetGuid("TeamCode", personPeriod.TeamCode)
            .SetString("TeamName", personPeriod.TeamName)
            .SetInt32("SiteId", personPeriod.SiteId)
            .SetGuid("SiteCode", personPeriod.SiteCode)
            .SetString("SiteName", personPeriod.SiteName)
            .SetInt32("BusinessUnitId", personPeriod.BusinessUnitId)
            .SetGuid("BusinessUnitCode", personPeriod.BusinessUnitCode)
            .SetString("BusinessUnitName", personPeriod.BusinessUnitName)
            .SetInt32("SkillsetId", personPeriod.SkillsetId)
            .SetString("Email", personPeriod.Email)
            .SetString("Note", personPeriod.Note)
            .SetDateTime("EmploymentStartDate", personPeriod.EmploymentStartDate)
            .SetDateTime("EmploymentEndDate", personPeriod.EmploymentEndDate)
            .SetInt32("TimeZoneId", personPeriod.TimeZoneId)
            .SetBoolean("IsAgent", personPeriod.IsAgent)
            .SetBoolean("IsUser", personPeriod.IsUser)
            .SetInt32("DatasourceId", personPeriod.DatasourceId)
            .SetDateTime("InsertDate", personPeriod.InsertDate)
            .SetDateTime("UpdateDate", personPeriod.UpdateDate)
            .SetDateTime("DatasourceUpdateDate", personPeriod.DatasourceUpdateDate)
            .SetBoolean("ToBeDeleted", personPeriod.ToBeDeleted)
            .SetString("WindowsDomain", personPeriod.WindowsDomain)
            .SetString("WindowsUsername", personPeriod.WindowsUsername)
            .SetInt32("ValidToDateIdMaxDate", personPeriod.ValidToDateIdMaxDate)
            .SetInt32("ValidToIntervalIdMaxDate", personPeriod.ValidToIntervalIdMaxDate)
            .SetInt32("ValidFromDateIdLocal", personPeriod.ValidFromDateIdLocal)
            .SetInt32("ValidToDateIdLocal", personPeriod.ValidToDateIdLocal)
            .SetDateTime("ValidFromDateLocal", personPeriod.ValidFromDateLocal)
            .SetDateTime("ValidToDateLocal", personPeriod.ValidToDateLocal)

                    .ExecuteUpdate();
            }
        }

        private IUnitOfWorkFactory statisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
            return identity.DataSource.Analytics;
        }
    }

    public class AnalyticsSkill : IAnalyticsSkill
    {
        public int SkillId { get; set; }
        public Guid SkillCode { get; set; }
        public string SkillName { get; set; }
        public int TimeZoneId { get; set; }
        public Guid ForecastMethodCode { get; set; }
        public string ForecastMethodName { get; set; }
        public int BusinessUnitId { get; set; }
        public int DatasourceId { get; set; }
        public DateTime InsertDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime DatasourceUpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }

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
