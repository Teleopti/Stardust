using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

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
				$@"SELECT {allDimPersonFields()}
					from mart.dim_person WITH (NOLOCK) WHERE person_code =:{nameof(personCode)} ")
				.SetGuid(nameof(personCode), personCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsPersonPeriod)))
				.SetReadOnly(true)
				.List<AnalyticsPersonPeriod>();
		}

		public AnalyticsPersonPeriod PersonPeriod(Guid personPeriodCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT {allDimPersonFields()}
					from mart.dim_person WITH (NOLOCK) WHERE person_period_code=:{nameof(personPeriodCode)} ")
				.SetGuid(nameof(personPeriodCode), personPeriodCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsPersonPeriod)))
				.SetReadOnly(true)
				.UniqueResult<AnalyticsPersonPeriod>();
		}

		public int GetOrCreateSite(Guid siteCode, string siteName, int businessUnitId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
						$@"mart.etl_dim_site_id_get @site_code=:{nameof(siteCode)}, @site_name=:{nameof(siteName)}, @business_unit_id=:{nameof(businessUnitId)}")
					.SetParameter(nameof(siteCode), siteCode)
					.SetParameter(nameof(siteName), siteName)
					.SetParameter(nameof(businessUnitId), businessUnitId)
					.UniqueResult<int>();
		}

		public void AddPersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_person_insert]
                     @person_code=:{nameof(AnalyticsPersonPeriod.PersonCode)}
                    ,@valid_from_date=:{nameof(AnalyticsPersonPeriod.ValidFromDate)}
                    ,@valid_to_date=:{nameof(AnalyticsPersonPeriod.ValidToDate)}
                    ,@valid_from_date_id=:{nameof(AnalyticsPersonPeriod.ValidFromDateId)}
                    ,@valid_from_interval_id=:{nameof(AnalyticsPersonPeriod.ValidFromIntervalId)}
                    ,@valid_to_date_id=:{nameof(AnalyticsPersonPeriod.ValidToDateId)}
                    ,@valid_to_interval_id=:{nameof(AnalyticsPersonPeriod.ValidToIntervalId)}
                    ,@person_period_code=:{nameof(AnalyticsPersonPeriod.PersonPeriodCode)}
                    ,@person_name=:{nameof(AnalyticsPersonPeriod.PersonName)}
                    ,@first_name=:{nameof(AnalyticsPersonPeriod.FirstName)}
                    ,@last_name=:{nameof(AnalyticsPersonPeriod.LastName)}
                    ,@employment_number=:{nameof(AnalyticsPersonPeriod.EmploymentNumber)}
                    ,@employment_type_code=:{nameof(AnalyticsPersonPeriod.EmploymentTypeCode)}
                    ,@employment_type_name=:{nameof(AnalyticsPersonPeriod.EmploymentTypeName)}
                    ,@contract_code=:{nameof(AnalyticsPersonPeriod.ContractCode)}
                    ,@contract_name=:{nameof(AnalyticsPersonPeriod.ContractName)}
                    ,@parttime_code=:{nameof(AnalyticsPersonPeriod.ParttimeCode)}
                    ,@parttime_percentage=:{nameof(AnalyticsPersonPeriod.ParttimePercentage)}
                    ,@team_id=:{nameof(AnalyticsPersonPeriod.TeamId)}
                    ,@team_code=:{nameof(AnalyticsPersonPeriod.TeamCode)}
                    ,@team_name=:{nameof(AnalyticsPersonPeriod.TeamName)}
                    ,@site_id=:{nameof(AnalyticsPersonPeriod.SiteId)}
                    ,@site_code=:{nameof(AnalyticsPersonPeriod.SiteCode)}
                    ,@site_name=:{nameof(AnalyticsPersonPeriod.SiteName)}
                    ,@business_unit_id=:{nameof(AnalyticsPersonPeriod.BusinessUnitId)}
                    ,@business_unit_code=:{nameof(AnalyticsPersonPeriod.BusinessUnitCode)}
                    ,@business_unit_name=:{nameof(AnalyticsPersonPeriod.BusinessUnitName)}
                    ,@skillset_id=:{nameof(AnalyticsPersonPeriod.SkillsetId)}
                    ,@email=:{nameof(AnalyticsPersonPeriod.Email)}
                    ,@note=:{nameof(AnalyticsPersonPeriod.Note)}
                    ,@employment_start_date=:{nameof(AnalyticsPersonPeriod.EmploymentStartDate)}
                    ,@employment_end_date=:{nameof(AnalyticsPersonPeriod.EmploymentEndDate)}
                    ,@time_zone_id=:{nameof(AnalyticsPersonPeriod.TimeZoneId)}
                    ,@is_agent=:{nameof(AnalyticsPersonPeriod.IsAgent)}
                    ,@is_user=:{nameof(AnalyticsPersonPeriod.IsUser)}
                    ,@datasource_id=:{nameof(AnalyticsPersonPeriod.DatasourceId)}
                    ,@insert_date=:{nameof(AnalyticsPersonPeriod.InsertDate)}
                    ,@update_date=:{nameof(AnalyticsPersonPeriod.UpdateDate)}
                    ,@datasource_update_date=:{nameof(AnalyticsPersonPeriod.DatasourceUpdateDate)}
                    ,@to_be_deleted=:{nameof(AnalyticsPersonPeriod.ToBeDeleted)}
                    ,@windows_domain=:{nameof(AnalyticsPersonPeriod.WindowsDomain)}
                    ,@windows_username=:{nameof(AnalyticsPersonPeriod.WindowsUsername)}
                    ,@valid_to_date_id_maxDate=:{nameof(AnalyticsPersonPeriod.ValidToDateIdMaxDate)}
                    ,@valid_to_interval_id_maxDate=:{nameof(AnalyticsPersonPeriod.ValidToIntervalIdMaxDate)}
                    ,@valid_from_date_id_local=:{nameof(AnalyticsPersonPeriod.ValidFromDateIdLocal)}
                    ,@valid_to_date_id_local=:{nameof(AnalyticsPersonPeriod.ValidToDateIdLocal)}
                    ,@valid_from_date_local=:{nameof(AnalyticsPersonPeriod.ValidFromDateLocal)}
                    ,@valid_to_date_local=:{nameof(AnalyticsPersonPeriod.ValidToDateLocal)}")
				.SetGuid(nameof(AnalyticsPersonPeriod.PersonCode), personPeriod.PersonCode)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidFromDate), personPeriod.ValidFromDate)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidToDate), personPeriod.ValidToDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromDateId), personPeriod.ValidFromDateId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromIntervalId), personPeriod.ValidFromIntervalId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateId), personPeriod.ValidToDateId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToIntervalId), personPeriod.ValidToIntervalId)
				.SetGuid(nameof(AnalyticsPersonPeriod.PersonPeriodCode), personPeriod.PersonPeriodCode)
				.SetString(nameof(AnalyticsPersonPeriod.PersonName), personPeriod.PersonName)
				.SetString(nameof(AnalyticsPersonPeriod.FirstName), personPeriod.FirstName)
				.SetString(nameof(AnalyticsPersonPeriod.LastName), personPeriod.LastName)
				.SetString(nameof(AnalyticsPersonPeriod.EmploymentNumber), personPeriod.EmploymentNumber)
				.SetString(nameof(AnalyticsPersonPeriod.EmploymentTypeName), personPeriod.EmploymentTypeName)
				.SetGuid(nameof(AnalyticsPersonPeriod.ContractCode), personPeriod.ContractCode)
				.SetString(nameof(AnalyticsPersonPeriod.ContractName), personPeriod.ContractName)
				.SetGuid(nameof(AnalyticsPersonPeriod.ParttimeCode), personPeriod.ParttimeCode)
				.SetString(nameof(AnalyticsPersonPeriod.ParttimePercentage), personPeriod.ParttimePercentage)
				.SetInt32(nameof(AnalyticsPersonPeriod.TeamId), personPeriod.TeamId)
				.SetGuid(nameof(AnalyticsPersonPeriod.TeamCode), personPeriod.TeamCode)
				.SetString(nameof(AnalyticsPersonPeriod.TeamName), personPeriod.TeamName)
				.SetInt32(nameof(AnalyticsPersonPeriod.SiteId), personPeriod.SiteId)
				.SetGuid(nameof(AnalyticsPersonPeriod.SiteCode), personPeriod.SiteCode)
				.SetString(nameof(AnalyticsPersonPeriod.SiteName), personPeriod.SiteName)
				.SetInt32(nameof(AnalyticsPersonPeriod.BusinessUnitId), personPeriod.BusinessUnitId)
				.SetGuid(nameof(AnalyticsPersonPeriod.BusinessUnitCode), personPeriod.BusinessUnitCode)
				.SetString(nameof(AnalyticsPersonPeriod.BusinessUnitName), personPeriod.BusinessUnitName)
				.SetString(nameof(AnalyticsPersonPeriod.Email), personPeriod.Email)
				.SetString(nameof(AnalyticsPersonPeriod.Note), personPeriod.Note)
				.SetDateTime(nameof(AnalyticsPersonPeriod.EmploymentStartDate), personPeriod.EmploymentStartDate)
				.SetDateTime(nameof(AnalyticsPersonPeriod.EmploymentEndDate), personPeriod.EmploymentEndDate)
				.SetBoolean(nameof(AnalyticsPersonPeriod.IsAgent), personPeriod.IsAgent)
				.SetBoolean(nameof(AnalyticsPersonPeriod.IsUser), personPeriod.IsUser)
				.SetInt32(nameof(AnalyticsPersonPeriod.DatasourceId), personPeriod.DatasourceId)
				.SetDateTime(nameof(AnalyticsPersonPeriod.InsertDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsPersonPeriod.UpdateDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsPersonPeriod.DatasourceUpdateDate), personPeriod.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsPersonPeriod.ToBeDeleted), personPeriod.ToBeDeleted)
				.SetString(nameof(AnalyticsPersonPeriod.WindowsDomain), personPeriod.WindowsDomain)
				.SetString(nameof(AnalyticsPersonPeriod.WindowsUsername), personPeriod.WindowsUsername)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateIdMaxDate), personPeriod.ValidToDateIdMaxDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToIntervalIdMaxDate), personPeriod.ValidToIntervalIdMaxDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromDateIdLocal), personPeriod.ValidFromDateIdLocal)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateIdLocal), personPeriod.ValidToDateIdLocal)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidFromDateLocal), personPeriod.ValidFromDateLocal)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidToDateLocal), personPeriod.ValidToDateLocal);

			setNullableValues(personPeriod, query);
			query.ExecuteUpdate();
		}

		public void DeletePersonPeriod(AnalyticsPersonPeriod analyticsPersonPeriod)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_person_set_to_be_deleted] @person_period_code=:{nameof(AnalyticsPersonPeriod.PersonPeriodCode)}")
				.SetGuid(nameof(AnalyticsPersonPeriod.PersonPeriodCode), analyticsPersonPeriod.PersonPeriodCode);
			query.ExecuteUpdate();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForPerson(int personId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT [acd_login_id] {nameof(AnalyticsBridgeAcdLoginPerson.AcdLoginId)}
							,[person_id] {nameof(AnalyticsBridgeAcdLoginPerson.PersonId)}
							,[team_id] {nameof(AnalyticsBridgeAcdLoginPerson.TeamId)}
							,[business_unit_id] {nameof(AnalyticsBridgeAcdLoginPerson.BusinessUnitId)}
							,[datasource_id] {nameof(AnalyticsBridgeAcdLoginPerson.DatasourceId)}
							,[insert_date] {nameof(AnalyticsBridgeAcdLoginPerson.InsertDate)}
							,[update_date] {nameof(AnalyticsBridgeAcdLoginPerson.UpdateDate)}
							,[datasource_update_date] {nameof(AnalyticsBridgeAcdLoginPerson.DatasourceUpdateDate)}
						 from mart.[bridge_acd_login_person] WITH (NOLOCK) 
						WHERE person_id =:{nameof(personId)} ")
				.SetInt32(nameof(personId), personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsBridgeAcdLoginPerson)))
				.SetReadOnly(true)
				.List<AnalyticsBridgeAcdLoginPerson>();
		}

		public IList<AnalyticsBridgeAcdLoginPerson> GetBridgeAcdLoginPersonsForAcdLoginPersons(int acdLoginId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT [acd_login_id] {nameof(AnalyticsBridgeAcdLoginPerson.AcdLoginId)}
							,[person_id] {nameof(AnalyticsBridgeAcdLoginPerson.PersonId)}
							,[team_id] {nameof(AnalyticsBridgeAcdLoginPerson.TeamId)}
							,[business_unit_id] {nameof(AnalyticsBridgeAcdLoginPerson.BusinessUnitId)}
							,[datasource_id] {nameof(AnalyticsBridgeAcdLoginPerson.DatasourceId)}
							,[insert_date] {nameof(AnalyticsBridgeAcdLoginPerson.InsertDate)}
							,[update_date] {nameof(AnalyticsBridgeAcdLoginPerson.UpdateDate)}
							,[datasource_update_date] {nameof(AnalyticsBridgeAcdLoginPerson.DatasourceUpdateDate)}
						 from mart.[bridge_acd_login_person] WITH (NOLOCK) 
						WHERE acd_login_id=:{nameof(acdLoginId)}")
				.SetInt32(nameof(acdLoginId), acdLoginId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsBridgeAcdLoginPerson)))
				.SetReadOnly(true)
				.List<AnalyticsBridgeAcdLoginPerson>();
		}

		public void AddBridgeAcdLoginPerson(AnalyticsBridgeAcdLoginPerson bridgeAcdLoginPerson)
		{
			var insertAndUpdateDateTime = DateTime.Now;
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_acd_login_person_insert]
                     @acd_login_id=:{nameof(AnalyticsBridgeAcdLoginPerson.AcdLoginId)}
                    ,@person_id=:{nameof(AnalyticsBridgeAcdLoginPerson.PersonId)}
					,@team_id=:{nameof(AnalyticsBridgeAcdLoginPerson.TeamId)}
                    ,@business_unit_id=:{nameof(AnalyticsBridgeAcdLoginPerson.BusinessUnitId)}
                    ,@datasource_id=:{nameof(AnalyticsBridgeAcdLoginPerson.DatasourceId)}
                    ,@insert_date=:{nameof(AnalyticsBridgeAcdLoginPerson.InsertDate)}
                    ,@update_date=:{nameof(AnalyticsBridgeAcdLoginPerson.UpdateDate)}
                    ,@datasource_update_date=:{nameof(AnalyticsBridgeAcdLoginPerson.DatasourceUpdateDate)}")
				.SetInt32(nameof(AnalyticsBridgeAcdLoginPerson.AcdLoginId), bridgeAcdLoginPerson.AcdLoginId)
				.SetInt32(nameof(AnalyticsBridgeAcdLoginPerson.PersonId), bridgeAcdLoginPerson.PersonId)
				.SetInt32(nameof(AnalyticsBridgeAcdLoginPerson.TeamId), bridgeAcdLoginPerson.TeamId)
				.SetInt32(nameof(AnalyticsBridgeAcdLoginPerson.BusinessUnitId), bridgeAcdLoginPerson.BusinessUnitId)
				.SetInt32(nameof(AnalyticsBridgeAcdLoginPerson.DatasourceId), bridgeAcdLoginPerson.DatasourceId)
				.SetDateTime(nameof(AnalyticsBridgeAcdLoginPerson.InsertDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsBridgeAcdLoginPerson.UpdateDate), insertAndUpdateDateTime)
				.SetDateTime(nameof(AnalyticsBridgeAcdLoginPerson.DatasourceUpdateDate), bridgeAcdLoginPerson.DatasourceUpdateDate);

			query.ExecuteUpdate();
		}

		public void DeleteBridgeAcdLoginPerson(int acdLoginId, int personId)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_acd_login_person_delete]
                     @acd_login_id=:{nameof(acdLoginId)}
                    ,@person_id=:{nameof(personId)}")
				.SetInt32(nameof(acdLoginId), acdLoginId)
				.SetInt32(nameof(personId), personId);
			query.ExecuteUpdate();
		}

		public void UpdatePersonNames(CommonNameDescriptionSetting commonNameDescriptionSetting, Guid businessUnitCode)
		{
			_analyticsUnitOfWork.Current()
				.Session()
				.CreateSQLQuery(commonNameDescriptionSetting.BuildSqlUpdateForAnalytics())
				.SetParameter("BusinessUnit", businessUnitCode)
				.ExecuteUpdate();
		}

		public void UpdatePersonPeriod(AnalyticsPersonPeriod personPeriod)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_person_update]
					 @person_id=:{nameof(AnalyticsPersonPeriod.PersonId)}
                    ,@valid_from_date=:{nameof(AnalyticsPersonPeriod.ValidFromDate)}
                    ,@valid_to_date=:{nameof(AnalyticsPersonPeriod.ValidToDate)}
                    ,@valid_from_date_id=:{nameof(AnalyticsPersonPeriod.ValidFromDateId)}
                    ,@valid_from_interval_id=:{nameof(AnalyticsPersonPeriod.ValidFromIntervalId)}
                    ,@valid_to_date_id=:{nameof(AnalyticsPersonPeriod.ValidToDateId)}
                    ,@valid_to_interval_id=:{nameof(AnalyticsPersonPeriod.ValidToIntervalId)}
                    ,@person_name=:{nameof(AnalyticsPersonPeriod.PersonName)}
                    ,@first_name=:{nameof(AnalyticsPersonPeriod.FirstName)}
                    ,@last_name=:{nameof(AnalyticsPersonPeriod.LastName)}
                    ,@employment_number=:{nameof(AnalyticsPersonPeriod.EmploymentNumber)}
                    ,@employment_type_code=:{nameof(AnalyticsPersonPeriod.EmploymentTypeCode)}
                    ,@employment_type_name=:{nameof(AnalyticsPersonPeriod.EmploymentTypeName)}
                    ,@contract_code=:{nameof(AnalyticsPersonPeriod.ContractCode)}
                    ,@contract_name=:{nameof(AnalyticsPersonPeriod.ContractName)}
                    ,@parttime_code=:{nameof(AnalyticsPersonPeriod.ParttimeCode)}
                    ,@parttime_percentage=:{nameof(AnalyticsPersonPeriod.ParttimePercentage)}
                    ,@team_id=:{nameof(AnalyticsPersonPeriod.TeamId)}
                    ,@team_code=:{nameof(AnalyticsPersonPeriod.TeamCode)}
                    ,@team_name=:{nameof(AnalyticsPersonPeriod.TeamName)}
                    ,@site_id=:{nameof(AnalyticsPersonPeriod.SiteId)}
                    ,@site_code=:{nameof(AnalyticsPersonPeriod.SiteCode)}
                    ,@site_name=:{nameof(AnalyticsPersonPeriod.SiteName)}
                    ,@business_unit_id=:{nameof(AnalyticsPersonPeriod.BusinessUnitId)}
                    ,@business_unit_code=:{nameof(AnalyticsPersonPeriod.BusinessUnitCode)}
                    ,@business_unit_name=:{nameof(AnalyticsPersonPeriod.BusinessUnitName)}
                    ,@skillset_id=:{nameof(AnalyticsPersonPeriod.SkillsetId)}
                    ,@email=:{nameof(AnalyticsPersonPeriod.Email)}
                    ,@note=:{nameof(AnalyticsPersonPeriod.Note)}
                    ,@employment_start_date=:{nameof(AnalyticsPersonPeriod.EmploymentStartDate)}
                    ,@employment_end_date=:{nameof(AnalyticsPersonPeriod.EmploymentEndDate)}
                    ,@time_zone_id=:{nameof(AnalyticsPersonPeriod.TimeZoneId)}
                    ,@is_agent=:{nameof(AnalyticsPersonPeriod.IsAgent)}
                    ,@is_user=:{nameof(AnalyticsPersonPeriod.IsUser)}
                    ,@datasource_id=:{nameof(AnalyticsPersonPeriod.DatasourceId)}
                    ,@update_date=:{nameof(AnalyticsPersonPeriod.UpdateDate)}
                    ,@datasource_update_date=:{nameof(AnalyticsPersonPeriod.DatasourceUpdateDate)}
                    ,@to_be_deleted=:{nameof(AnalyticsPersonPeriod.ToBeDeleted)}
                    ,@windows_domain=:{nameof(AnalyticsPersonPeriod.WindowsDomain)}
                    ,@windows_username=:{nameof(AnalyticsPersonPeriod.WindowsUsername)}
                    ,@valid_to_date_id_maxDate=:{nameof(AnalyticsPersonPeriod.ValidToDateIdMaxDate)}
                    ,@valid_to_interval_id_maxDate=:{nameof(AnalyticsPersonPeriod.ValidToIntervalIdMaxDate)}
                    ,@valid_from_date_id_local=:{nameof(AnalyticsPersonPeriod.ValidFromDateIdLocal)}
                    ,@valid_to_date_id_local=:{nameof(AnalyticsPersonPeriod.ValidToDateIdLocal)}
                    ,@valid_from_date_local=:{nameof(AnalyticsPersonPeriod.ValidFromDateLocal)}
                    ,@valid_to_date_local=:{nameof(AnalyticsPersonPeriod.ValidToDateLocal)}")
				.SetInt32(nameof(AnalyticsPersonPeriod.PersonId), personPeriod.PersonId)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidFromDate), personPeriod.ValidFromDate)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidToDate), personPeriod.ValidToDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromDateId), personPeriod.ValidFromDateId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromIntervalId), personPeriod.ValidFromIntervalId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateId), personPeriod.ValidToDateId)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToIntervalId), personPeriod.ValidToIntervalId)
				.SetString(nameof(AnalyticsPersonPeriod.PersonName), personPeriod.PersonName)
				.SetString(nameof(AnalyticsPersonPeriod.FirstName), personPeriod.FirstName)
				.SetString(nameof(AnalyticsPersonPeriod.LastName), personPeriod.LastName)
				.SetString(nameof(AnalyticsPersonPeriod.EmploymentNumber), personPeriod.EmploymentNumber)
				.SetString(nameof(AnalyticsPersonPeriod.EmploymentTypeName), personPeriod.EmploymentTypeName)
				.SetGuid(nameof(AnalyticsPersonPeriod.ContractCode), personPeriod.ContractCode)
				.SetString(nameof(AnalyticsPersonPeriod.ContractName), personPeriod.ContractName)
				.SetGuid(nameof(AnalyticsPersonPeriod.ParttimeCode), personPeriod.ParttimeCode)
				.SetString(nameof(AnalyticsPersonPeriod.ParttimePercentage), personPeriod.ParttimePercentage)
				.SetInt32(nameof(AnalyticsPersonPeriod.TeamId), personPeriod.TeamId)
				.SetGuid(nameof(AnalyticsPersonPeriod.TeamCode), personPeriod.TeamCode)
				.SetString(nameof(AnalyticsPersonPeriod.TeamName), personPeriod.TeamName)
				.SetInt32(nameof(AnalyticsPersonPeriod.SiteId), personPeriod.SiteId)
				.SetGuid(nameof(AnalyticsPersonPeriod.SiteCode), personPeriod.SiteCode)
				.SetString(nameof(AnalyticsPersonPeriod.SiteName), personPeriod.SiteName)
				.SetInt32(nameof(AnalyticsPersonPeriod.BusinessUnitId), personPeriod.BusinessUnitId)
				.SetGuid(nameof(AnalyticsPersonPeriod.BusinessUnitCode), personPeriod.BusinessUnitCode)
				.SetString(nameof(AnalyticsPersonPeriod.BusinessUnitName), personPeriod.BusinessUnitName)
				.SetInt32(nameof(AnalyticsPersonPeriod.SkillsetId), personPeriod.SkillsetId.GetValueOrDefault())
				.SetString(nameof(AnalyticsPersonPeriod.Email), personPeriod.Email)
				.SetString(nameof(AnalyticsPersonPeriod.Note), personPeriod.Note)
				.SetDateTime(nameof(AnalyticsPersonPeriod.EmploymentStartDate), personPeriod.EmploymentStartDate)
				.SetDateTime(nameof(AnalyticsPersonPeriod.EmploymentEndDate), personPeriod.EmploymentEndDate)

				.SetBoolean(nameof(AnalyticsPersonPeriod.IsAgent), personPeriod.IsAgent)
				.SetBoolean(nameof(AnalyticsPersonPeriod.IsUser), personPeriod.IsUser)
				.SetInt32(nameof(AnalyticsPersonPeriod.DatasourceId), personPeriod.DatasourceId)
				.SetDateTime(nameof(AnalyticsPersonPeriod.UpdateDate), DateTime.Now)
				.SetDateTime(nameof(AnalyticsPersonPeriod.DatasourceUpdateDate), personPeriod.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsPersonPeriod.ToBeDeleted), personPeriod.ToBeDeleted)
				.SetString(nameof(AnalyticsPersonPeriod.WindowsDomain), personPeriod.WindowsDomain)
				.SetString(nameof(AnalyticsPersonPeriod.WindowsUsername), personPeriod.WindowsUsername)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateIdMaxDate), personPeriod.ValidToDateIdMaxDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToIntervalIdMaxDate), personPeriod.ValidToIntervalIdMaxDate)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidFromDateIdLocal), personPeriod.ValidFromDateIdLocal)
				.SetInt32(nameof(AnalyticsPersonPeriod.ValidToDateIdLocal), personPeriod.ValidToDateIdLocal)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidFromDateLocal), personPeriod.ValidFromDateLocal)
				.SetDateTime(nameof(AnalyticsPersonPeriod.ValidToDateLocal), personPeriod.ValidToDateLocal);

			setNullableValues(personPeriod, query);

			query.ExecuteUpdate();
		}

		private static void setNullableValues(AnalyticsPersonPeriod personPeriod, IQuery query)
		{
			if (personPeriod.SkillsetId.HasValue)
			{
				query.SetInt32(nameof(AnalyticsPersonPeriod.SkillsetId), personPeriod.SkillsetId.Value);
			}
			else
			{
				query.SetParameter(nameof(AnalyticsPersonPeriod.SkillsetId), null, NHibernateUtil.Int32);
			}

			if (personPeriod.EmploymentTypeCode.HasValue)
			{
				query.SetInt32(nameof(AnalyticsPersonPeriod.EmploymentTypeCode), personPeriod.EmploymentTypeCode.Value);
			}
			else
			{
				query.SetParameter(nameof(AnalyticsPersonPeriod.EmploymentTypeCode), null, NHibernateUtil.Int32);
			}

			if (personPeriod.TimeZoneId.HasValue)
			{
				query.SetInt32(nameof(AnalyticsPersonPeriod.TimeZoneId), personPeriod.TimeZoneId.Value);
			}
			else
			{
				query.SetParameter(nameof(AnalyticsPersonPeriod.TimeZoneId), null, NHibernateUtil.Int32);
			}
		}

		private static string allDimPersonFields()
		{
			return $@"person_id {nameof(AnalyticsPersonPeriod.PersonId)}
                        , person_code {nameof(AnalyticsPersonPeriod.PersonCode)}
                        , valid_from_date {nameof(AnalyticsPersonPeriod.ValidFromDate)}
                        , valid_to_date {nameof(AnalyticsPersonPeriod.ValidToDate)}
                        , valid_from_date_id {nameof(AnalyticsPersonPeriod.ValidFromDateId)}
                        , valid_to_date_id {nameof(AnalyticsPersonPeriod.ValidToDateId)}
                        , valid_to_interval_id {nameof(AnalyticsPersonPeriod.ValidToIntervalId)}
                        , person_period_code {nameof(AnalyticsPersonPeriod.PersonPeriodCode)}
                        , person_name {nameof(AnalyticsPersonPeriod.PersonName)}
                        , first_name {nameof(AnalyticsPersonPeriod.FirstName)}
                        , last_name {nameof(AnalyticsPersonPeriod.LastName)}
                        , employment_number {nameof(AnalyticsPersonPeriod.EmploymentNumber)}
                        , employment_type_code {nameof(AnalyticsPersonPeriod.EmploymentTypeCode)}
                        , employment_type_name {nameof(AnalyticsPersonPeriod.EmploymentTypeName)}
                        , contract_code {nameof(AnalyticsPersonPeriod.ContractCode)}
                        , contract_name {nameof(AnalyticsPersonPeriod.ContractName)}
                        , parttime_code {nameof(AnalyticsPersonPeriod.ParttimeCode)}
                        , parttime_percentage {nameof(AnalyticsPersonPeriod.ParttimePercentage)}
                        , team_id {nameof(AnalyticsPersonPeriod.TeamId)}
                        , team_code {nameof(AnalyticsPersonPeriod.TeamCode)}
                        , team_name {nameof(AnalyticsPersonPeriod.TeamName)}
                        , site_id {nameof(AnalyticsPersonPeriod.SiteId)}
                        , site_code {nameof(AnalyticsPersonPeriod.SiteCode)}
                        , site_name {nameof(AnalyticsPersonPeriod.SiteName)}
                        , business_unit_id {nameof(AnalyticsPersonPeriod.BusinessUnitId)}
                        , business_unit_code {nameof(AnalyticsPersonPeriod.BusinessUnitCode)}
                        , business_unit_name {nameof(AnalyticsPersonPeriod.BusinessUnitName)}
                        , skillset_id {nameof(AnalyticsPersonPeriod.SkillsetId)}
                        , email {nameof(AnalyticsPersonPeriod.Email)}
                        , note {nameof(AnalyticsPersonPeriod.Note)}
                        , employment_start_date {nameof(AnalyticsPersonPeriod.EmploymentStartDate)}
                        , employment_end_date {nameof(AnalyticsPersonPeriod.EmploymentEndDate)}
                        , time_zone_id {nameof(AnalyticsPersonPeriod.TimeZoneId)}
                        , is_agent {nameof(AnalyticsPersonPeriod.IsAgent)}
                        , is_user {nameof(AnalyticsPersonPeriod.IsUser)}
                        , datasource_id {nameof(AnalyticsPersonPeriod.DatasourceId)}
                        , insert_date {nameof(AnalyticsPersonPeriod.InsertDate)}
                        , update_date {nameof(AnalyticsPersonPeriod.UpdateDate)}
                        , datasource_update_date {nameof(AnalyticsPersonPeriod.DatasourceUpdateDate)}
                        , to_be_deleted {nameof(AnalyticsPersonPeriod.ToBeDeleted)}
                        , windows_domain {nameof(AnalyticsPersonPeriod.WindowsDomain)}
                        , windows_username {nameof(AnalyticsPersonPeriod.WindowsUsername)} 
						, valid_to_date_id_maxDate {nameof(AnalyticsPersonPeriod.ValidToDateIdMaxDate)}
						, valid_to_interval_id_maxDate {nameof(AnalyticsPersonPeriod.ValidToIntervalIdMaxDate)}
						, valid_from_date_id_local {nameof(AnalyticsPersonPeriod.ValidFromDateIdLocal)}
						, valid_to_date_id_local {nameof(AnalyticsPersonPeriod.ValidToDateIdLocal)}
						, valid_from_date_local {nameof(AnalyticsPersonPeriod.ValidFromDateLocal)}
						, valid_to_date_local {nameof(AnalyticsPersonPeriod.ValidToDateLocal)}";
		}

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"SELECT 
						person_id {nameof(AnalyticsPersonBusinessUnit.PersonId)}, 
						business_unit_id {nameof(AnalyticsPersonBusinessUnit.BusinessUnitId)} 
					FROM mart.dim_person WITH (NOLOCK) 
					WHERE person_period_code =:{nameof(personPeriodCode)}")
				.SetGuid(nameof(personPeriodCode), personPeriodCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsPersonBusinessUnit)))
				.SetReadOnly(true)
				//.SetTimeout(120)
				.UniqueResult<IAnalyticsPersonBusinessUnit>();
		}

		public void UpdateValidToLocalDateIds(IAnalyticsDate maxDate)
		{
			_analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"update dp
						set 
						  dp.valid_to_date_local = :{nameof(maxDate.DateDate)},
						  dp.valid_to_date_id_local = :{nameof(maxDate.DateId)}
						from mart.dim_person dp
						  where dp.valid_to_date_id = -2 and dp.person_id >= 0")
				.SetParameter(nameof(maxDate.DateDate), maxDate.DateDate)
				.SetParameter(nameof(maxDate.DateId), maxDate.DateId)
				.ExecuteUpdate();
		}
	}
}