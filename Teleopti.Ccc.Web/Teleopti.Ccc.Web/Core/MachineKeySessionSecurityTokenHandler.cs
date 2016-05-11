using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.Core
{
	public class MachineKeySessionSecurityTokenHandler : SessionSecurityTokenHandler
	{
		public MachineKeySessionSecurityTokenHandler()
			: base(createTransforms())
		{ }

		public MachineKeySessionSecurityTokenHandler(SecurityTokenCache cache, TimeSpan tokenLifetime)
			: base(createTransforms(), cache, tokenLifetime)
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