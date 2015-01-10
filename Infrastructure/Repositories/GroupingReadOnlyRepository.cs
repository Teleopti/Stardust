using NHibernate.Impl;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
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
					.SetDateTime("currentDate", queryDate.Date)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl) _currentUnitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			object businessUnitId;
			if (!filter.Parameters.TryGetValue("businessUnitParameter", out businessUnitId))
			{
				businessUnitId = ((ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
			}
			return Guid.Parse(businessUnitId.ToString());
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
					.SetDateTime("currentDate", queryDate.Date)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
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
					.SetDateTime("currentDate", queryDate.Date)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ReadOnlyGroupDetail)))
					.SetReadOnly(true)
					.List<ReadOnlyGroupDetail>();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateGroupingReadModel(ICollection<Guid> inputIds)
        {
            //change the array to comma seperated string
            string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());
	        _currentUnitOfWork.Session().CreateSQLQuery(
		        "exec [ReadModel].[UpdateGroupingReadModel] :idList").SetString("idList", ids).ExecuteUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateGroupingReadModelGroupPage(ICollection<Guid> inputIds)
        {
            //change the array to comma seperated string
			string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());
			_currentUnitOfWork.Session().CreateSQLQuery(
            		"exec [ReadModel].[UpdateGroupingReadModelGroupPage] :idList").SetString("idList", ids).ExecuteUpdate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public void UpdateGroupingReadModelData(ICollection<Guid> inputIds)
        {
            //change the array to comma seperated string
			string ids = String.Join(",", inputIds.Select(p => p.ToString()).ToArray());

	        _currentUnitOfWork.Session().CreateSQLQuery(
		        "exec [ReadModel].[UpdateGroupingReadModelData] :idList").SetString("idList", ids).ExecuteUpdate();
        }
	}

	public class ReadOnlyGroupDetail : IAuthorizeOrganisationDetail
	{
		public Guid PageId { get; set; }
		public string GroupName { get; set; }
		public Guid GroupId { get; set; }
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string EmploymentNumber { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public class ReadOnlyGroupPage
	{
		public string PageName { get; set; }
		public Guid PageId { get; set; }
	}
}
