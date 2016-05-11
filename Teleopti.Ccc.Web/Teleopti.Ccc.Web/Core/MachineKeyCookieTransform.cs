using System.IdentityModel;
using System.Web.Security;

namespace Teleopti.Ccc.Web.Core
{
	public class MachineKeyCookieTransform : CookieTransform
	{
		public override byte[] Decode(byte[] encoded)
		{
			return MachineKey.Unprotect(encoded);
		}

		public override byte[] Encode(byte[] value)
		{
			return MachineKey.Protect(value);
		}
	}
}