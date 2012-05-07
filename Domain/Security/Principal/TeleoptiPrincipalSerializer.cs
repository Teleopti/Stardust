using System.IO;
using System.IdentityModel.Claims;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipalSerializer
	{
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

		public void Serialize(TeleoptiPrincipalSerializable principal, Stream stream)
		{
			Serializer.WriteObject(stream, principal);
		}

		public TeleoptiPrincipalSerializable Deserialize(Stream stream)
		{
			var principal = (TeleoptiPrincipalSerializable)Serializer.ReadObject(stream);
			var identity = (TeleoptiIdentity) principal.Identity;
			identity.WindowsIdentity = WindowsIdentity.GetCurrent();
			return principal;
		}
	}
}