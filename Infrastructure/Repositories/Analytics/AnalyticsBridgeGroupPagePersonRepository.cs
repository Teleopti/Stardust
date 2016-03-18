using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBridgeGroupPagePersonRepository : IAnalyticsBridgeGroupPagePersonRepository
	{
		private IAnalyticsUnitOfWorkFactory statisticUnitOfWorkFactory()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			return identity.DataSource.Analytics;
		}

		public void DeleteAllBridgeGroupPagePerson(IEnumerable<Guid> groupPageIds)
		{
			if (!groupPageIds.Any())
				return;
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in groupPageIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_delete_all]
                     @group_page_codes=:GroupPageCodes")
					.SetString("GroupPageCodes", string.Join(",", batch));
					query.ExecuteUpdate();
				}
			}
		}

		public void DeleteBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupPageId)
		{
			if (!personIds.Any())
				return;
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in personIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
						@"exec mart.[etl_bridge_group_page_person_delete]
							@person_codes=:PersonIds,
							@group_page_code=:GroupPageId")
						.SetString("PersonIds", string.Join(",", batch))
						.SetGuid("GroupPageId", groupPageId);
					query.ExecuteUpdate();
				}
			}
		}

		public IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupPageId)
		{
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
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
						where gp.group_code = :GroupPageCode")
					.SetGuid("GroupPageCode", groupPageId)
					.List<Guid>();
			}
		}

		public void AddBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupPageId)
		{
			if (!personIds.Any())
				return;
			using (var uow = statisticUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{
				foreach (var batch in personIds.Batch(100))
				{
					var query = uow.Session().CreateSQLQuery(
					@"exec mart.[etl_bridge_group_page_person_insert]
                    @person_codes=:PersonIds,
					@group_page_code=:GroupPageId")
					.SetString("PersonIds", string.Join(",", batch))
					.SetGuid("GroupPageId", groupPageId);
					query.ExecuteUpdate();
				}
			}
		}
	}
}