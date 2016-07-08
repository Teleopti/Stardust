using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsGroupPageRepository : IAnalyticsGroupPageRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsGroupPageRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public IEnumerable<AnalyticsGroup> GetGroupPage(Guid groupPageCode)
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
                from mart.[dim_group_page] WITH (NOLOCK) where group_page_code=:{nameof(groupPageCode)}")
				.SetGuid(nameof(groupPageCode), groupPageCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsGroup)))
				.SetReadOnly(true)
				.List<AnalyticsGroup>();
		}

		public AnalyticsGroup GetGroupPageByGroupCode(Guid groupCode)
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
                from mart.[dim_group_page] WITH (NOLOCK) where group_code=:{nameof(groupCode)}")
				.SetGuid(nameof(groupCode), groupCode)
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
                ,@group_is_custom=:{nameof(AnalyticsGroup.GroupIsCustom)}")
				.SetGuid(nameof(AnalyticsGroup.GroupPageCode), analyticsGroup.GroupPageCode)
				.SetString(nameof(AnalyticsGroup.GroupPageName), analyticsGroup.GroupPageName)
				.SetString(nameof(AnalyticsGroup.GroupPageNameResourceKey), analyticsGroup.GroupPageNameResourceKey)
				.SetGuid(nameof(AnalyticsGroup.GroupCode), analyticsGroup.GroupCode)
				.SetString(nameof(AnalyticsGroup.GroupName), analyticsGroup.GroupName)
				.SetBoolean(nameof(AnalyticsGroup.GroupIsCustom), analyticsGroup.GroupIsCustom);
			query.ExecuteUpdate();
		}

		public void DeleteGroupPagesByGroupCodes(IEnumerable<Guid> groupCodes)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_delete_by_group_codes]
                    @group_codes=:{nameof(groupCodes)}")
				.SetString(nameof(groupCodes), string.Join(",", groupCodes));
			query.ExecuteUpdate();
		}

		public IEnumerable<AnalyticsGroupPage> GetBuildInGroupPageBase()
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select distinct 
					[group_page_id] {nameof(AnalyticsGroupPage.GroupPageId)}
					,[group_page_code] {nameof(AnalyticsGroupPage.GroupPageCode)}
					,[group_page_name_resource_key] {nameof(AnalyticsGroupPage.GroupPageNameResourceKey)}
					,[group_page_name] {nameof(AnalyticsGroupPage.GroupPageName)}
					from mart.[dim_group_page] WITH (NOLOCK)
					where [group_page_name_resource_key] is not null")
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

		public void DeleteGroupPages(IEnumerable<Guid> groupPageIds)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_group_page_delete]
                    @group_page_codes=:{nameof(groupPageIds)}")
				.SetString(nameof(groupPageIds), string.Join(",", groupPageIds));
			query.ExecuteUpdate();
		}
	}
}