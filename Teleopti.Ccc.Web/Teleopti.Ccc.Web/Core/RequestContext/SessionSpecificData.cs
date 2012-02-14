using System;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionSpecificData
	{
		public SessionSpecificData(Guid businessUnitId, string dataSourceName, Guid personId)
		{
			BusinessUnitId = businessUnitId;
			DataSourceName = dataSourceName;
			PersonId = personId;
		}

		public Guid BusinessUnitId { get; private set; }
		public string DataSourceName { get; private set; }
		public Guid PersonId { get; private set; }		

	}
}