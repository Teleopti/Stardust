﻿using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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

		public ReadOnlyGroupPage GetGroupPage(Guid groupPageId)
		{
			const string sql =
				"SELECT DISTINCT PageName,PageId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "+ 
				" And PageId=:pageId";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql).SetGuid("businessUnitId", getBusinessUnitId())
				.SetGuid("pageId", groupPageId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupPage)))
				.SetReadOnly(true)
				.UniqueResult<ReadOnlyGroupPage>();
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

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(ReadOnlyGroupPage groupPage, DateOnly queryDate)
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
				+ "AND (LeavingDate >= :startDate OR LeavingDate IS NULL) "
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

		public IEnumerable<ReadOnlyGroupDetail> FindGroups(IEnumerable<Guid> groupIds, DateOnlyPeriod period)
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
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND GroupId in (:groupIds)"
				+ "AND :startDate <= isnull(EndDate, '2059-12-31') "
				+ "AND :endDate >= StartDate "
				+ "AND (LeavingDate >= :startDate OR LeavingDate IS NULL) "
				+ "ORDER BY groupname";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetString("groupIds", string.Join(",", groupIds))
				.SetDateOnly("startDate", period.StartDate)
				.SetDateOnly("endDate", period.EndDate)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
				.SetReadOnly(true)
				.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForPeople(IEnumerable<Guid> peopleIdCollection)
		{
			const string sql =
				"SELECT DISTINCT PersonId,FirstName,LastName,EmploymentNumber,TeamId,SiteId,BusinessUnitId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND PageId IN (:pageId) "
				+ "AND PersonId IN (:personIdCollection)";
			var result = new List<ReadOnlyGroupDetail>();
			foreach (var people in peopleIdCollection.Batch(1500))
			{
				result.AddRange(_currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("pageId", Group.PageMainId)
					.SetParameterList("personIdCollection", people.ToArray())
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>());
			}
			return result;
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnly queryDate)
		{
			const string sql =
				"exec [ReadModel].[LoadPeopleInGroups] @businessUnitId=:businessUnitId, @startDate=:startDate, @endDate=:endDate, @groupIds=:groupIds";

			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetDateOnly("startDate", queryDate)
				.SetDateOnly("endDate", queryDate)
				.SetString("groupIds", groupId.ToString())
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroup(Guid groupId, DateOnlyPeriod queryRange)
		{
			const string sql =
				"exec [ReadModel].[LoadPeopleInGroups] @businessUnitId=:businessUnitId, @startDate=:startDate, @endDate=:endDate, @groupIds=:groupIds";

			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetDateOnly("startDate", queryRange.StartDate)
					.SetDateOnly("endDate", queryRange.EndDate)
					.SetString("groupIds", groupId.ToString())
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

		public IEnumerable<ReadOnlyGroupPage> AvailableGroupsBasedOnPeriod(DateOnlyPeriod period)
		{
			const string sql =
				"SELECT DISTINCT PageName,PageId "
				+ "FROM ReadModel.groupingreadonly "
				+ "WHERE businessunitid=:businessUnitId "
				+ "AND :startDate <= EndDate AND :endDate >= StartDate ";
			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetDateOnly("startDate", period.StartDate)
					.SetDateOnly("endDate", period.EndDate)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupPage)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupPage>();
		}

		public IEnumerable<ReadOnlyGroupDetail> AvailableGroups(DateOnlyPeriod period, params Guid[] groupPageIds)
		{
			const string sql =
				"exec [ReadModel].[LoadAvailableGroups] @businessUnitId=:businessUnitId, @startDate=:startDate, @endDate=:endDate, @pageIds=:pageIds";

			var pageIds = string.Join(",", groupPageIds);

			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetDateOnly("startDate", period.StartDate)
				.SetDateOnly("endDate", period.EndDate)
				.SetString("pageIds", pageIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
				.SetReadOnly(true)
				.List<ReadOnlyGroupDetail>();
		}

		public IEnumerable<ReadOnlyGroupDetail> DetailsForGroups(Guid[] groupIds, DateOnlyPeriod queryRange)
		{
			const string sql =
				"exec [ReadModel].[LoadPeopleInGroups] @businessUnitId=:businessUnitId, @startDate=:startDate, @endDate=:endDate, @groupIds=:groupIds";

			return _currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetGuid("businessUnitId", getBusinessUnitId())
				.SetDateOnly("startDate", queryRange.StartDate)
				.SetDateOnly("endDate", queryRange.EndDate)
				.SetString("groupIds",string.Join(",", groupIds))
				.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
				.SetReadOnly(true)
				.List<ReadOnlyGroupDetail>();
		}
	}

}
