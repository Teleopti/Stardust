using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsGroupPageRepository : IAnalyticsGroupPageRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsGroupPageRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IEnumerable<AnalyticsGroup> GetGroupPage(Guid groupPageCode, Guid businessUnitCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                [group_page_id] {nameof(AnalyticsGroup.GroupPageId)}
					,[group_page_code] {nameof(AnalyticsGroup.GroupPageCode)}
					,[group_page_name] {nameof(AnalyticsGroup.GroupPageName)}
					,[group_page_name_resource_key] {nameof(AnalyticsGroup.GroupPageNameResourceKey)}
					,[group_id] {nameof(AnalyticsGroup.GroupId)}
					,[group_code] {nameof(AnalyticsGroup.GroupCode)}
					,[group_name] {nameof(AnalyticsGroup.GroupName)}
					,[group_is_custom] {nameof(AnalyticsGroup.GroupIsCustom)}
					,[business_unit_id] {nameof(AnalyticsGroup.BusinessUnitId)}
					,[business_unit_code] {nameof(AnalyticsGroup.BusinessUnitCode)}
					,[business_unit_name] {nameof(AnalyticsGroup.BusinessUnitName)}
					,[datasource_id] {nameof(AnalyticsGroup.DatasourceId)}
					,[insert_date] {nameof(AnalyticsGroup.InsertDate)}
					,[datasource_update_date] {nameof(AnalyticsGroup.DatasourceUpdateDate)}
                from mart.[dim_group_page] WITH (NOLOCK) 
				where group_page_code=:{nameof(groupPageCode)} AND business_unit_code=:{nameof(businessUnitCode)}")
				.SetGuid(nameof(groupPageCode), groupPageCode)
				.SetGuid(nameof(businessUnitCode), businessUnitCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsGroup)))
				.SetReadOnly(true)
				.List<AnalyticsGroup>();
		}

		public AnalyticsGroup GetGroupPageByGroupCode(Guid groupCode, Guid businessUnitCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select 
	                [group_page_id] {nameof(AnalyticsGroup.GroupPageId)}
					,[group_page_code] {nameof(AnalyticsGroup.GroupPageCode)}
					,[group_page_name] {nameof(AnalyticsGroup.GroupPageName)}
					,[group_page_name_resource_key] {nameof(AnalyticsGroup.GroupPageNameResourceKey)}
					,[group_id] {nameof(AnalyticsGroup.GroupId)}
					,[group_code] {nameof(AnalyticsGroup.GroupCode)}
					,[group_name] {nameof(AnalyticsGroup.GroupName)}
					,[group_is_custom] {nameof(AnalyticsGroup.GroupIsCustom)}
					,[business_unit_id] {nameof(AnalyticsGroup.BusinessUnitId)}
					,[business_unit_code] {nameof(AnalyticsGroup.BusinessUnitCode)}
					,[business_unit_name] {nameof(AnalyticsGroup.BusinessUnitName)}
					,[datasource_id] {nameof(AnalyticsGroup.DatasourceId)}
					,[insert_date] {nameof(AnalyticsGroup.InsertDate)}
					,[datasource_update_date] {nameof(AnalyticsGroup.DatasourceUpdateDate)}
                from mart.[dim_group_page] WITH (NOLOCK) 
				where group_code=:{nameof(groupCode)} AND business_unit_code=:{nameof(businessUnitCode)}")
				.SetGuid(nameof(groupCode), groupCode)
				.SetGuid(nameof(businessUnitCode), businessUnitCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGroup)))
				.SetReadOnly(true)
				.UniqueResult<AnalyticsGroup>();
		}

		public void UpdateGroupPage(AnalyticsGroup analyticsGroup)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_update]
                     @group_page_code=:{nameof(AnalyticsGroup.GroupPageCode)}
					,@group_page_name=:{nameof(AnalyticsGroup.GroupPageName)}
					,@group_page_name_resource_key=:{nameof(AnalyticsGroup.GroupPageNameResourceKey)}
					,@group_code=:{nameof(AnalyticsGroup.GroupCode)}
					,@group_name=:{nameof(AnalyticsGroup.GroupName)}
					,@group_is_custom=:{nameof(AnalyticsGroup.GroupIsCustom)}
					,@business_unit_code=:{nameof(AnalyticsGroup.BusinessUnitCode)}")
				.SetGuid(nameof(AnalyticsGroup.GroupPageCode), analyticsGroup.GroupPageCode)
				.SetString(nameof(AnalyticsGroup.GroupPageName), analyticsGroup.GroupPageName)
				.SetString(nameof(AnalyticsGroup.GroupPageNameResourceKey), analyticsGroup.GroupPageNameResourceKey)
				.SetGuid(nameof(AnalyticsGroup.GroupCode), analyticsGroup.GroupCode)
				.SetString(nameof(AnalyticsGroup.GroupName), analyticsGroup.GroupName)
				.SetBoolean(nameof(AnalyticsGroup.GroupIsCustom), analyticsGroup.GroupIsCustom)
				.SetGuid(nameof(AnalyticsGroup.BusinessUnitCode), analyticsGroup.BusinessUnitCode);
			query.ExecuteUpdate();
		}

		public void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes, Guid businessUnitCode)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_delete_by_group_codes]
                     @group_codes=:{nameof(groupCodes)}
					,@business_unit_code=:{nameof(businessUnitCode)}")
				.SetString(nameof(groupCodes), string.Join(",", groupCodes))
				.SetParameter(nameof(businessUnitCode), businessUnitCode);
			query.ExecuteUpdate();
		}

		public IEnumerable<AnalyticsGroupPage> GetBuildInGroupPageBase(Guid businessUnitCode)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select distinct 
					[group_page_id] {nameof(AnalyticsGroupPage.GroupPageId)}
					,[group_page_code] {nameof(AnalyticsGroupPage.GroupPageCode)}
					,[group_page_name_resource_key] {nameof(AnalyticsGroupPage.GroupPageNameResourceKey)}
					,[group_page_name] {nameof(AnalyticsGroupPage.GroupPageName)}
					from mart.[dim_group_page] WITH (NOLOCK)
					where [group_page_name_resource_key] is not null AND business_unit_code=:{nameof(businessUnitCode)}")
				.SetParameter(nameof(businessUnitCode), businessUnitCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AnalyticsGroupPage)))
				.SetReadOnly(true)
				.List<AnalyticsGroupPage>();
		}

		public void AddGroupPageIfNotExisting(AnalyticsGroup analyticsGroup)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_insert]
                    @group_page_code=:{nameof(AnalyticsGroup.GroupPageCode)}
					,@group_page_name=:{nameof(AnalyticsGroup.GroupPageName)}
					,@group_page_name_resource_key=:{nameof(AnalyticsGroup.GroupPageNameResourceKey)}
					,@group_code=:{nameof(AnalyticsGroup.GroupCode)}
					,@group_name=:{nameof(AnalyticsGroup.GroupName)}
					,@group_is_custom=:{nameof(AnalyticsGroup.GroupIsCustom)}
					,@business_unit_code=:{nameof(AnalyticsGroup.BusinessUnitCode)}")
				.SetGuid(nameof(AnalyticsGroup.GroupPageCode), analyticsGroup.GroupPageCode)
				.SetString(nameof(AnalyticsGroup.GroupPageName), analyticsGroup.GroupPageName)
				.SetString(nameof(AnalyticsGroup.GroupPageNameResourceKey), analyticsGroup.GroupPageNameResourceKey)
				.SetGuid(nameof(AnalyticsGroup.GroupCode), analyticsGroup.GroupCode)
				.SetString(nameof(AnalyticsGroup.GroupName), analyticsGroup.GroupName)
				.SetBoolean(nameof(AnalyticsGroup.GroupIsCustom), analyticsGroup.GroupIsCustom)
				.SetGuid(nameof(AnalyticsGroup.BusinessUnitCode), analyticsGroup.BusinessUnitCode);
			query.ExecuteUpdate();
		}

		public void DeleteGroupPages(IEnumerable<Guid> groupPageIds, Guid businessUnitCode)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_delete]
                     @group_page_codes=:{nameof(groupPageIds)}
					,@business_unit_code=:{nameof(businessUnitCode)}")
				.SetString(nameof(groupPageIds), string.Join(",", groupPageIds))
				.SetParameter(nameof(businessUnitCode), businessUnitCode);
			query.ExecuteUpdate();
		}
	}
}