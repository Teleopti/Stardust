using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel;
using System.IdentityModel.Tokens;

namespace Teleopti.Ccc.Web.AuthenticationBridge.Controllers
{
	public class MachineKeySessionSecurityTokenHandler : SessionSecurityTokenHandler
	{
		public MachineKeySessionSecurityTokenHandler()
			: base(createTransforms())
		{ }

		public MachineKeySessionSecurityTokenHandler(TimeSpan tokenLifetime)
			: base(createTransforms(), tokenLifetime)
		{ }

		private static ReadOnlyCollection<CookieTransform> createTransforms()
		{
			return new List<CookieTransform>
			{
				new DeflateCookieTransform(),
				new MachineKeyCookieTransform()
			}.AsReadOnly();
		}
	}
}