using System;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	[Serializable]
	public class SessionSpecificData
	{
		public SessionSpecificData(Guid businessUnitId, string dataSourceName, Guid personId, string tenantPassword, bool isTeleoptiApplicationLogon)
		{
			BusinessUnitId = businessUnitId;
			DataSourceName = dataSourceName;
			PersonId = personId;
			TenantPassword = tenantPassword;
			IsTeleoptiApplicationLogon = isTeleoptiApplicationLogon;
		}

		public Guid BusinessUnitId { get; private set; }
		public string DataSourceName { get; private set; }
		public Guid PersonId { get; private set; }
		public string TenantPassword { get; set; }
		public bool IsTeleoptiApplicationLogon { get; private set; }
	}
}