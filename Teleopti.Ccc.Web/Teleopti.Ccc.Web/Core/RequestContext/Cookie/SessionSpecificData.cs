using System;

namespace Teleopti.Ccc.Web.Core.RequestContext.Cookie
{
	[Serializable]
	public class SessionSpecificData
	{
		public SessionSpecificData(Guid businessUnitId, string dataSourceName, Guid personId, string tenantPassword)
		{
			BusinessUnitId = businessUnitId;
			DataSourceName = dataSourceName;
			PersonId = personId;
			TenantPassword = tenantPassword;
		}

		public Guid BusinessUnitId { get; private set; }
		public string DataSourceName { get; private set; }
		public Guid PersonId { get; private set; }
		public string TenantPassword { get; set; }
	}
}