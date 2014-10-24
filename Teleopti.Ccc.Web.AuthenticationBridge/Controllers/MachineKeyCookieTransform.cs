using System.Text;
using System.Web.Security;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.AuthenticationBridge.Controllers
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