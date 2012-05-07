using System.IO;
using System.IdentityModel.Claims;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalSerializer
	{
		private readonly IApplicationData _applicationData;

		private static readonly DataContractSerializer Serializer = new DataContractSerializer(
			typeof (TeleoptiPrincipalSerializable),
			"TeleoptiPrincipal",
			"http://schemas.ccc.teleopti.com/sdk/2012/05/",
			new[]
				{
					typeof (TeleoptiIdentity),
					typeof (DefaultClaimSet),
					typeof (Regional),
					typeof (CccTimeZoneInfo),
					typeof (OrganisationMembership)
				},
			1024, // just a number...
			false,
			true,
			null
			);

		public TeleoptiPrincipalSerializer(IApplicationData applicationData)
		{
			_applicationData = applicationData;
		}

		public void Serialize(TeleoptiPrincipalSerializable principal, Stream stream)
		{
			Serializer.WriteObject(stream, principal);
		}

		public TeleoptiPrincipalSerializable Deserialize(Stream stream)
		{
			var principal = (TeleoptiPrincipalSerializable)Serializer.ReadObject(stream);
			var identity = (TeleoptiIdentity) principal.Identity;
			identity.WindowsIdentity = WindowsIdentity.GetCurrent();
			identity.DataSource = GetDataSourceByName(identity.DataSourceName);
			return principal;
		}

		private IDataSource GetDataSourceByName(string dataSourceName)
		{
			if (_applicationData == null)
				return null;
			return (
			       	from d in _applicationData.RegisteredDataSourceCollection
			       	where d.DataSourceName == dataSourceName
			       	select d)
				.SingleOrDefault();
		}
	}
}