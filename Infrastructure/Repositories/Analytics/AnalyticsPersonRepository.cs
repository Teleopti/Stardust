using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
    public class AnalyticsPersonRepository : IAnalyticsPersonPeriodRepository
    {
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
                    .SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsPersonPeriod)))
                    .SetReadOnly(true)
                    //.SetTimeout(120)
                    .List<IAnalyticsPersonPeriod>();
            }
        }

        private IUnitOfWorkFactory statisticUnitOfWorkFactory()
        {
            var identity = ((ITeleoptiIdentity) TeleoptiPrincipal.CurrentPrincipal.Identity);
            return identity.DataSource.Analytics;
        }
    }

    public class AnalyticsPersonPeriod : IAnalyticsPersonPeriod
    {
        public int PersonId { get; set; }
        public Guid PersonCode { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public int ValidFromDateId { get; set; }
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
    }
}
