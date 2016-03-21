using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsGroupPageRepository : IAnalyticsGroupPageRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsGroupPageRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public IEnumerable<AnalyticsGroupPage> GetGroupPage(Guid groupPageCode)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"select 
	                    [group_page_id] GroupPageId
					  ,[group_page_code] GroupPageCode
					  ,[group_page_name] GroupPageName
					  ,[group_page_name_resource_key] GroupPageNameResourceKey
					  ,[group_id] GroupId
					  ,[group_code] GroupCode
					  ,[group_name] GroupName
					  ,[group_is_custom] GroupIsCustom
					  ,[business_unit_id] BusinessUnitId
					  ,[business_unit_code] BusinessUnitCode
					  ,[business_unit_name] BusinessUnitName
					  ,[datasource_id] DatasourceId
					  ,[insert_date] InsertDate
					  ,[datasource_update_date] DatasourceUpdateDate
                    from mart.[dim_group_page] WITH (NOLOCK) where group_page_code=:GroupPageCode")
					.SetGuid("GroupPageCode", groupPageCode)
					.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsGroupPage)))
					.SetReadOnly(true)
					.List<AnalyticsGroupPage>();
			}
		}

		public void UpdateGroupPage(AnalyticsGroupPage analyticsGroupPage)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_dim_group_page_update]
                     @group_page_code=:GroupPageCode
                    ,@group_page_name=:GroupPageName
                    ,@group_page_name_resource_key=:GroupPageNameResourceKey
                    ,@group_code=:GroupCode
                    ,@group_name=:GroupName
                    ,@group_is_custom=:GroupIsCustom")
					.SetGuid("GroupPageCode", analyticsGroupPage.GroupPageCode)
					.SetString("GroupPageName", analyticsGroupPage.GroupPageName)
					.SetString("GroupPageNameResourceKey", analyticsGroupPage.GroupPageNameResourceKey)
					.SetGuid("GroupCode", analyticsGroupPage.GroupCode)
					.SetString("GroupName", analyticsGroupPage.GroupName)
					.SetBoolean("GroupIsCustom", analyticsGroupPage.GroupIsCustom);
				query.ExecuteUpdate();
			}
		}

		public void AddGroupPage(AnalyticsGroupPage analyticsGroupPage)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_dim_group_page_insert]
                     @group_page_code=:GroupPageCode
                    ,@group_page_name=:GroupPageName
                    ,@group_page_name_resource_key=:GroupPageNameResourceKey
                    ,@group_code=:GroupCode
                    ,@group_name=:GroupName
                    ,@group_is_custom=:GroupIsCustom
					,@business_unit_code=:BusinessUnitCode")
					.SetGuid("GroupPageCode", analyticsGroupPage.GroupPageCode)
					.SetString("GroupPageName", analyticsGroupPage.GroupPageName)
					.SetString("GroupPageNameResourceKey", analyticsGroupPage.GroupPageNameResourceKey)
					.SetGuid("GroupCode", analyticsGroupPage.GroupCode)
					.SetString("GroupName", analyticsGroupPage.GroupName)
					.SetBoolean("GroupIsCustom", analyticsGroupPage.GroupIsCustom)
					.SetGuid("BusinessUnitCode", analyticsGroupPage.BusinessUnitCode);
				query.ExecuteUpdate();
			}
		}

		public void DeleteGroupPages(IEnumerable<Guid> groupPageIds)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_dim_group_page_delete]
                     @group_page_codes=:GroupPageCodes")
					.SetString("GroupPageCodes", string.Join(",", groupPageIds));
				query.ExecuteUpdate();
			}
		}
	}
}