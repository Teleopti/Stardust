using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionSpecificDataStringSerializer : ISessionSpecificDataStringSerializer
	{
		public string Serialize(SessionSpecificData data)
		{
			return string.Format("{0}{1:N}{2:N}{3}", (int)data.AuthenticationType, data.BusinessUnitId, data.PersonId, data.DataSourceName);
		}

		public SessionSpecificData Deserialize(string stringData)
		{
			if (string.IsNullOrEmpty(stringData) || stringData.Length <= 64)
				return null;

			var authType = (AuthenticationTypeOption)Enum.Parse(typeof(AuthenticationTypeOption), stringData.Substring(0, 1));
			return new SessionSpecificData(Guid.ParseExact(stringData.Substring(1, 32), "N"),
			                               stringData.Substring(65),
			                               Guid.ParseExact(stringData.Substring(33, 32), "N"),
			                               authType);
		}
	}
}