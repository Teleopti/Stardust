using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsBridgeGroupPagePersonRepository : IAnalyticsBridgeGroupPagePersonRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;

		public AnalyticsBridgeGroupPagePersonRepository(ICurrentAnalyticsUnitOfWork analyticsUnitOfWork)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
		}

		public void DeleteAllBridgeGroupPagePerson(IEnumerable<Guid> groupPageIds)
		{
			if (!groupPageIds.Any())
				return;

			foreach (var batch in groupPageIds.Batch(100))
			{
				var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_group_page_person_delete_all]
                    @group_page_codes=:{nameof(groupPageIds)}")
				.SetString(nameof(groupPageIds), string.Join(",", batch));
				query.ExecuteUpdate();
			}
		}

		public void DeleteBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId)
		{
			if (!personIds.Any())
				return;
			foreach (var batch in personIds.Batch(100))
			{
				var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"exec mart.[etl_bridge_group_page_person_delete]
						@person_codes=:{nameof(personIds)},
						@group_page_code=:{nameof(groupId)}")
					.SetString(nameof(personIds), string.Join(",", batch))
					.SetGuid(nameof(groupId), groupId);
				query.ExecuteUpdate();
			}
		}

		public IEnumerable<Guid> GetBridgeGroupPagePerson(Guid groupId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select p.person_code 
					from [mart].[bridge_group_page_person] bgpp 
					WITH (NOLOCK) 
					join [mart].[dim_person] p 
					WITH (NOLOCK) 
					on bgpp.person_id = p.person_id 
					join [mart].[dim_group_page] gp 
					WITH (NOLOCK) 
					on bgpp.group_page_id = gp.group_page_id 
					where gp.group_code=:{nameof(groupId)}")
				.SetGuid(nameof(groupId), groupId)
				.List<Guid>();
		}

		public IEnumerable<Guid> GetGroupPagesForPersonPeriod(Guid personPeriodId)
		{
			return _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"select gp.group_code
					from [mart].[bridge_group_page_person] bgpp 
					WITH (NOLOCK) 
					join [mart].[dim_person] p 
					WITH (NOLOCK) 
					on bgpp.person_id = p.person_id 
					join [mart].[dim_group_page] gp 
					WITH (NOLOCK) 
					on bgpp.group_page_id = gp.group_page_id 
					where p.person_period_code = :{nameof(personPeriodId)}")
				.SetGuid(nameof(personPeriodId), personPeriodId)
				.List<Guid>();
		}

		public void DeleteBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds)
		{
			if (!groupIds.Any())
				return;

			foreach (var batch in groupIds.Batch(100))
			{
				var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
					$@"exec mart.[etl_bridge_group_page_person_delete_for_person_period]
						@person_period_code=:{nameof(personPeriodId)},
						@group_codes=:{nameof(groupIds)}")
					.SetGuid(nameof(personPeriodId), personPeriodId)
					.SetString(nameof(groupIds), string.Join(",", batch));
				query.ExecuteUpdate();
			}
		}

		public void AddBridgeGroupPagePersonForPersonPeriod(Guid personPeriodId, IEnumerable<Guid> groupIds)
		{
			if (!groupIds.Any())
				return;

			foreach (var batch in groupIds.Batch(100))
			{
				var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_group_page_person_insert_for_person_period]
					@person_period_code=:{nameof(personPeriodId)},
					@group_codes=:{nameof(groupIds)}")
				.SetGuid(nameof(personPeriodId), personPeriodId)
				.SetString(nameof(groupIds), string.Join(",", batch));
				query.ExecuteUpdate();
			}
		}

		public void DeleteBridgeGroupPagePersonExcludingPersonPeriods(Guid personId, IEnumerable<Guid> personPeriodIds)
		{
			var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_group_page_person_delete_removed]
					@person_code=:{nameof(personId)},
					@person_period_codes=:{nameof(personPeriodIds)}")
				.SetGuid(nameof(personId), personId)
				.SetString(nameof(personPeriodIds), string.Join(",", personPeriodIds));
			query.ExecuteUpdate();
		}

		public void AddBridgeGroupPagePerson(IEnumerable<Guid> personIds, Guid groupId)
		{
			if (!personIds.Any())
				return;
			foreach (var batch in personIds.Batch(100))
			{
				var query = _analyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_bridge_group_page_person_insert]
					@person_codes=:{nameof(personIds)},
					@group_page_code=:{nameof(groupId)}")
				.SetString(nameof(personIds), string.Join(",", batch))
				.SetGuid(nameof(groupId), groupId);
				query.ExecuteUpdate();
			}
		}
	}
}