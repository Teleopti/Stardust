using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsPersonPeriodRepository : IAnalyticsPersonPeriodRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsPersonPeriodRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IList<AnalyticsPersonPeriod> GetPersonPeriods(Guid personCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
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
				.List<AnalyticsPersonPeriod>();
		}

		public AnalyticsPersonPeriod PersonPeriod(Guid personPeriodCode)
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
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
                        , windows_username WindowsUsername from mart.dim_person WITH (NOLOCK) WHERE person_period_code=:code ")
				.SetGuid("code", personPeriodCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsPersonPeriod)))
				.SetReadOnly(true)
				.UniqueResult<AnalyticsPersonPeriod>();
		}

		public int SiteId(Guid siteCode, string siteName, int businessUnitId)
		{

			return
				_analyticsUnitOfWork.Current()
					.Session()
					.CreateSQLQuery(
						@"mart.etl_dim_site_id_get @site_code=:siteCode , @site_name=:siteName, @business_unit_id=:businessUnitId")
					.SetGuid("siteCode", siteCode)
					.SetString("siteName", siteName)
					.SetInt32("businessUnitId", businessUnitId)
					.UniqueResult<int>();
		}

		public int? TimeZone(string timeZoneCode)
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select TOP 1 time_zone_id from mart.dim_time_zone WITH (NOLOCK) WHERE time_zone_code=:TimeZoneCode")
				.SetString("TimeZoneCode", timeZoneCode)
				.UniqueResult<short?>();
		}

		public IAnalyticsDate MaxDate()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select max(date_id) DateId, max(date_date) DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_id>=0")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate MinDate()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select min(date_id) DateId, min(date_date) DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_id>=0")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public IAnalyticsDate Date(DateTime date)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select date_id DateId, date_date DateDate FROM mart.dim_date WITH (NOLOCK) WHERE date_date=:dateDate")
				.SetDateTime("dateDate", date.Date)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsDate)))
				.SetReadOnly(true)
				.UniqueResult<IAnalyticsDate>();
		}

		public void AddPersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
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
				.SetString("Email", personPeriod.Email)
				.SetString("Note", personPeriod.Note)
				.SetDateTime("EmploymentStartDate", personPeriod.EmploymentStartDate)
				.SetDateTime("EmploymentEndDate", personPeriod.EmploymentEndDate)
				.SetBoolean("IsAgent", personPeriod.IsAgent)
				.SetBoolean("IsUser", personPeriod.IsUser)
				.SetInt32("DatasourceId", personPeriod.DatasourceId)
				.SetDateTime("InsertDate", insertAndUpdateDateTime)
				.SetDateTime("UpdateDate", insertAndUpdateDateTime)
				.SetDateTime("DatasourceUpdateDate", personPeriod.DatasourceUpdateDate)
				.SetBoolean("ToBeDeleted", personPeriod.ToBeDeleted)
				.SetString("WindowsDomain", personPeriod.WindowsDomain)
				.SetString("WindowsUsername", personPeriod.WindowsUsername)
				.SetInt32("ValidToDateIdMaxDate", personPeriod.ValidToDateIdMaxDate)
				.SetInt32("ValidToIntervalIdMaxDate", personPeriod.ValidToIntervalIdMaxDate)
				.SetInt32("ValidFromDateIdLocal", personPeriod.ValidFromDateIdLocal)
				.SetInt32("ValidToDateIdLocal", personPeriod.ValidToDateIdLocal)
				.SetDateTime("ValidFromDateLocal", personPeriod.ValidFromDateLocal)
				.SetDateTime("ValidToDateLocal", personPeriod.ValidToDateLocal);

			if (personPeriod.SkillsetId.HasValue)
			{
				query.SetInt32("SkillsetId", personPeriod.SkillsetId.Value);
			}
			else
			{
				query.SetParameter("SkillsetId", null, NHibernateUtil.Int32);
			}

			if (personPeriod.EmploymentTypeCode.HasValue)
			{
				query.SetInt32("EmploymentTypeCode", personPeriod.EmploymentTypeCode.Value);
			}
			else
			{
				query.SetParameter("EmploymentTypeCode", null, NHibernateUtil.Int32);
			}

			if (personPeriod.TimeZoneId.HasValue)
			{
				query.SetInt32("TimeZoneId", personPeriod.TimeZoneId.Value);
			}
			else
			{
				query.SetParameter("TimeZoneId", null, NHibernateUtil.Int32);
			}
			query.ExecuteUpdate();
		}

		public int IntervalsPerDay()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select count(*) from mart.dim_interval WITH (NOLOCK)")
				.UniqueResult<int>();
		}

		public int MaxIntervalId()
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				"select max(interval_id) from mart.dim_interval WITH (NOLOCK)")
				.UniqueResult<short>();
		}

		public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
		{

			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_person_set_to_be_deleted] @person_period_code=:PersonPeriodCode")
				.SetGuid("PersonPeriodCode", analyticsPersonPeriod.PersonPeriodCode);
			query.ExecuteUpdate();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForPerson(int personId)
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"SELECT [acd_login_id] AcdLoginId
							,[person_id] PersonId
							,[team_id] TeamId
							,[business_unit_id] BusinessUnitId
							,[datasource_id] DatasourceId
							,[insert_date] InsertDate
							,[update_date] UpdateDate
							,[datasource_update_date] DatasourceUpdateDate
						 from mart.[bridge_acd_login_person] WITH (NOLOCK) WHERE person_id =:PersonId ")
				.SetInt32("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsBridgeAcdLoginPerson)))
				.SetReadOnly(true)
				.List<AnalyticsBridgeAcdLoginPerson>();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForAcdLoginPersons(int acdLoginId)
		{

			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"SELECT [acd_login_id] AcdLoginId
							,[person_id] PersonId
							,[team_id] TeamId
							,[business_unit_id] BusinessUnitId
							,[datasource_id] DatasourceId
							,[insert_date] InsertDate
							,[update_date] UpdateDate
							,[datasource_update_date] DatasourceUpdateDate
						 from mart.[bridge_acd_login_person] WITH (NOLOCK) WHERE acd_login_id=:AcdLoginId")
				.SetInt32("AcdLoginId", acdLoginId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsBridgeAcdLoginPerson)))
				.SetReadOnly(true)
				.List<AnalyticsBridgeAcdLoginPerson>();
		}

		public void AddBridgeAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{

			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_bridge_acd_login_person_insert]
                     @acd_login_id=:AcdLoginId
                    ,@person_id=:PersonId
					,@team_id=:TeamId
                    ,@business_unit_id=:BusinessUnitId
                    ,@datasource_id=:DatasourceId
                    ,@insert_date=:InsertDate
                    ,@update_date=:UpdateDate
                    ,@datasource_update_date=:DatasourceUpdateDate")
				.SetInt32("AcdLoginId", bridgeAcdLoginPerson.AcdLoginId)
				.SetInt32("PersonId", bridgeAcdLoginPerson.PersonId)
				.SetInt32("TeamId", bridgeAcdLoginPerson.TeamId)
				.SetInt32("BusinessUnitId", bridgeAcdLoginPerson.BusinessUnitId)
				.SetInt32("DatasourceId", bridgeAcdLoginPerson.DatasourceId)
				.SetDateTime("InsertDate", insertAndUpdateDateTime)
				.SetDateTime("UpdateDate", insertAndUpdateDateTime)
				.SetDateTime("DatasourceUpdateDate", bridgeAcdLoginPerson.DatasourceUpdateDate);

			query.ExecuteUpdate();
		}

		public void DeleteBridgeAcdLoginPerson(int acdLoginId, int personId)
		{

			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_bridge_acd_login_person_delete]
                     @acd_login_id=:AcdLoginId
                    ,@person_id=:PersonId")
				.SetInt32("AcdLoginId", acdLoginId)
				.SetInt32("PersonId", personId);
			query.ExecuteUpdate();
		}

		public void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod)
		{

			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_person_update]
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
				.SetInt32("SkillsetId", personPeriod.SkillsetId.GetValueOrDefault())
				.SetString("Email", personPeriod.Email)
				.SetString("Note", personPeriod.Note)
				.SetDateTime("EmploymentStartDate", personPeriod.EmploymentStartDate)
				.SetDateTime("EmploymentEndDate", personPeriod.EmploymentEndDate)

				.SetBoolean("IsAgent", personPeriod.IsAgent)
				.SetBoolean("IsUser", personPeriod.IsUser)
				.SetInt32("DatasourceId", personPeriod.DatasourceId)
				.SetDateTime("UpdateDate", DateTime.Now)
				.SetDateTime("DatasourceUpdateDate", personPeriod.DatasourceUpdateDate)
				.SetBoolean("ToBeDeleted", personPeriod.ToBeDeleted)
				.SetString("WindowsDomain", personPeriod.WindowsDomain)
				.SetString("WindowsUsername", personPeriod.WindowsUsername)
				.SetInt32("ValidToDateIdMaxDate", personPeriod.ValidToDateIdMaxDate)
				.SetInt32("ValidToIntervalIdMaxDate", personPeriod.ValidToIntervalIdMaxDate)
				.SetInt32("ValidFromDateIdLocal", personPeriod.ValidFromDateIdLocal)
				.SetInt32("ValidToDateIdLocal", personPeriod.ValidToDateIdLocal)
				.SetDateTime("ValidFromDateLocal", personPeriod.ValidFromDateLocal)
				.SetDateTime("ValidToDateLocal", personPeriod.ValidToDateLocal);

			if (personPeriod.EmploymentTypeCode.HasValue)
			{
				query.SetInt32("EmploymentTypeCode", personPeriod.EmploymentTypeCode.Value);
			}
			else
			{
				query.SetParameter("EmploymentTypeCode", null, NHibernateUtil.Int32);
			}
			if (personPeriod.TimeZoneId.HasValue)
			{
				query.SetInt32("TimeZoneId", personPeriod.TimeZoneId.Value);
			}
			else
			{
				query.SetParameter("TimeZoneId", null, NHibernateUtil.Int32);
			}

			query.ExecuteUpdate();
		}
	}
}