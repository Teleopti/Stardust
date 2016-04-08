using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	[Serializable]
	public class AnalyticsPermission
	{
		public virtual Guid PersonCode { get; set; }
		public virtual int TeamId { get; set; }
		public virtual bool MyOwn { get; set; }
		public virtual int BusinessUnitId { get; set; }
		public virtual int DatasourceId { get; set; }
		public virtual DateTime DatasourceUpdateDate { get; set; }
		public virtual Guid ReportId { get; set; }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			var analyticsPermission = obj as AnalyticsPermission;
			if (analyticsPermission == null)
				return false;
			return PersonCode == analyticsPermission.PersonCode
				   && TeamId == analyticsPermission.TeamId
				   && MyOwn == analyticsPermission.MyOwn
				   && BusinessUnitId == analyticsPermission.BusinessUnitId
				   && DatasourceId == analyticsPermission.DatasourceId
				   && ReportId == analyticsPermission.ReportId;
		}
		public override int GetHashCode()
		{
			return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", PersonCode, TeamId, MyOwn, BusinessUnitId, DatasourceId, ReportId)
				.GetHashCode();
		}
	}
}