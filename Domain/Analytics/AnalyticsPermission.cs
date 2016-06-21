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
			var analyticsPermission = obj as AnalyticsPermission;
			if (analyticsPermission == null)
				return false;
			return PersonCode == analyticsPermission.PersonCode
				   && TeamId == analyticsPermission.TeamId
				   && MyOwn == analyticsPermission.MyOwn
				   && BusinessUnitId == analyticsPermission.BusinessUnitId
				   && ReportId == analyticsPermission.ReportId;
		}
		public override int GetHashCode()
		{
			return $"{PersonCode}|{TeamId}|{MyOwn}|{BusinessUnitId}|{ReportId}".GetHashCode();
		}
	}
}