using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBridgeGroupPagePersonRepository : IAnalyticsBridgeGroupPagePersonRepository
	{
		private readonly ICurrentDataSource _currentDataSource;

		public AnalyticsBridgeGroupPagePersonRepository(ICurrentDataSource currentDataSource)
		{
			_currentDataSource = currentDataSource;
		}

		public void DeleteAllBridgeGroupPagePerson(IEnumerable<Guid> groupPageIds)
		{
			if (!groupPageIds.Any())
				return;
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in groupPageIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_delete_all]
                     @group_page_codes=:GroupPageIds")
					.SetString("GroupPageIds", string.Join(",", batch));
					query.ExecuteUpdate();
				}
			}
		}

		public void DeleteBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId)
		{
			if (!personIds.Any())
				return;
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in personIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
						@"exec mart.[etl_bridge_group_page_person_delete]
							@person_codes=:PersonIds,
							@group_page_code=:GroupId")
						.SetString("PersonIds", string.Join(",", batch))
						.SetGuid("GroupId", groupId);
					query.ExecuteUpdate();
				}
			}
		}

		public IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"select p.person_code 
						from [mart].[bridge_group_page_person] bgpp 
						WITH (NOLOCK) 
						join [mart].[dim_person] p 
						WITH (NOLOCK) 
						on bgpp.person_id = p.person_id 
						join [mart].[dim_group_page] gp 
						WITH (NOLOCK) 
						on bgpp.group_page_id = gp.group_page_id 
						where gp.group_code = :GroupId")
					.SetGuid("GroupId", groupId)
					.List<Guid>();
			}
		}

		public IEnumerable<Guid> GetBuiltInGroupPagesForPersonPeriod(Guid personPeriodId)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					@"select gp.group_code
						from [mart].[bridge_group_page_person] bgpp 
						WITH (NOLOCK) 
						join [mart].[dim_person] p 
						WITH (NOLOCK) 
						on bgpp.person_id = p.person_id 
						join [mart].[dim_group_page] gp 
						WITH (NOLOCK) 
						on bgpp.group_page_id = gp.group_page_id 
						where p.person_period_code = :PersonPeriodId
						and gp.group_is_custom=0")
					.SetGuid("PersonPeriodId", personPeriodId)
					.List<Guid>();
			}
		}

		public void DeleteBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds)
		{
			if (!groupIds.Any())
				return;

			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in groupIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
						@"exec mart.[etl_bridge_group_page_person_delete_for_person_period]
						@person_period_code=:PersonPeriodId,
						@group_codes=:GroupIds")
						.SetGuid("PersonPeriodId", personPeriodId)
						.SetString("GroupIds", string.Join(",", batch));
					query.ExecuteUpdate();
				}
			}
		}

		public void AddBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds)
		{
			if (!groupIds.Any())
				return;
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in groupIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_insert_for_person_period]
                    @person_period_code=:PersonPeriodId,
					@group_codes=:GroupIds")
					.SetGuid("PersonPeriodId", personPeriodId)
					.SetString("GroupIds", string.Join(",", batch));
					query.ExecuteUpdate();
				}
			}
		}

		public void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personId, IEnumerable<Guid> personPeriodIds)
		{
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_delete_removed]
					@person_code=:PersonId,
					@person_period_codes=:PersonPeriodIds")
					.SetGuid("PersonId", personId)
					.SetString("PersonPeriodIds", string.Join(",", personPeriodIds));
				query.ExecuteUpdate();
			}
		}

		public void AddBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId)
		{
			if (!personIds.Any())
				return;
			using (var uow = _currentDataSource.Current().Analytics.CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in personIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_insert]
                    @person_codes=:PersonIds,
					@group_page_code=:GroupId")
					.SetString("PersonIds", string.Join(",", batch))
					.SetGuid("GroupId", groupId);
					query.ExecuteUpdate();
				}
			}
		}
	}
}