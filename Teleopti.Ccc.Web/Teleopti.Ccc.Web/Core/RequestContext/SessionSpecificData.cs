using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	[Serializable]
	public class SessionSpecificData
	{
		public SessionSpecificData(Guid businessUnitId, string dataSourceName, Guid personId, AuthenticationTypeOption authenticationType)
		{
			BusinessUnitId = businessUnitId;
			DataSourceName = dataSourceName;
			PersonId = personId;
			AuthenticationType = authenticationType;
		}

		public Guid BusinessUnitId { get; private set; }
		public string DataSourceName { get; private set; }
		public Guid PersonId { get; private set; }
		public AuthenticationTypeOption AuthenticationType { get; private set; }
	}
}