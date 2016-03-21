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