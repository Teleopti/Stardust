using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class GroupingReadOnlyRepository : IGroupingReadOnlyRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public GroupingReadOnlyRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupPages()
		{
			const string sql =
				"SELECT DISTINCT PageName,PageId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId ORDER BY pagename";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupPage)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupPage>();
		}
		
		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnly queryDate)
		{
			const string sql =
				"SELECT PageId, GroupName,GroupId,PersonId,FirstName,LastName,EmploymentNumber,TeamId,SiteId,BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND :currentDate BETWEEN StartDate and isnull(EndDate,'2059-12-31') "
				+ "AND (LeavingDate >= :currentDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetDateOnly("currentDate", queryDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

		private Guid getBusinessUnitId()
		{
			return ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault();
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage,DateOnly queryDate)
		{
			const string sql =
				"SELECT DISTINCT GroupName, "
				+ "GroupId, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) PersonId, "
				+ "'' FirstName, "
				+ "'' LastName, "
				+ "'' EmploymentNumber, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) TeamId, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) SiteId, "
				+ "BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId AND pageid=:pageId "
				+ "AND :currentDate BETWEEN StartDate and isnull(EndDate, '2059-12-31') "
				+ "AND (LeavingDate >= :currentDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("pageId", groupPage.PageId)
					.SetDateOnly("currentDate", queryDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnlyPeriod queryDateRange)
		{
			const string sql =
				"SELECT DISTINCT GroupName, "
				+ "GroupId, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) PersonId, "
				+ "'' FirstName, "
				+ "'' LastName, "
				+ "'' EmploymentNumber, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) TeamId, "
				+ "CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) SiteId, "
				+ "BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId AND pageid=:pageId "
				+ "AND :startDate <= isnull(EndDate, '2059-12-31') "
				+ "AND :endDate >= StartDate "
				+ "AND (LeavingDate >= :endDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("pageId", groupPage.PageId)
					.SetDateOnly("startDate", queryDateRange.StartDate)
					.SetDateOnly("endDate", queryDateRange.EndDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(IEnumerable<ReadOnlyGroupPage> groupPages, DateOnly queryDate)
		{
			const string sql =
				"exec [ReadModel].[LoadAvailableGroups] @businessUnitId=:businessUnitId, @date=:queryDate,@pageIds=:pageIds";

			var pageIds = string.Join(",", groupPages.Select(p => p.PageId).ToArray());

			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetDateOnly("queryDate", queryDate)
				.SetString("pageIds", pageIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof (ReadOnlyGroupDetail)))
				.SetReadOnly(true)
				.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate)
		{
			const string sql =
				"SELECT PersonId,FirstName,LastName,EmploymentNumber,TeamId,SiteId,BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND groupid=:groupId "
				+ "AND :currentDate BETWEEN StartDate and isnull(EndDate,'2059-12-31') "
				+ "AND (LeavingDate >= :currentDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("groupId", groupId)
					.SetDateOnly("currentDate", queryDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}
		
		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnlyPeriod queryRange)
		{
			const string sql =
				"SELECT PersonId,FirstName,LastName,EmploymentNumber,TeamId,SiteId,BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND groupid=:groupId "
				+ "AND :startDate <= isnull(EndDate, '2059-12-31') "
				+ "AND :endDate >= StartDate "
				+ "AND (LeavingDate >= :startDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("groupId", groupId)
					.SetDateOnly("startDate", queryRange.StartDate)
					.SetDateOnly("endDate", queryRange.EndDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}
		
		public void UpdateGroupingReadModel(ICollection<Guid> inputIds)
		{
			string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());
			_currentUnitOfWork.Session().CreateSQLQuery(
				"exec [ReadModel].[UpdateGroupingReadModel] :idList").SetString("idList", ids).ExecuteUpdate();
		}

		public void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds)
		{
			string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());
			_currentUnitOfWork.Session().CreateSQLQuery(
					"exec [ReadModel].[UpdateGroupingReadModelGroupPage] :idList").SetString("idList", ids).ExecuteUpdate();
		}

		public void UpdateGroupingReadModelData(ICollection<Guid> inputIds)
		{
			string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());

			_currentUnitOfWork.Session().CreateSQLQuery(
				"exec [ReadModel].[UpdateGroupingReadModelData] :idList").SetString("idList", ids).ExecuteUpdate();
		}
	}

}
