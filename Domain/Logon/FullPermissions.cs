using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public class FullPermissions
	{
		private readonly IAuthorizationScope _authorization;
		private readonly SystemUserAuthorization _systemUserAuthorization;

		public FullPermissions(
			IAuthorizationScope authorization,
			IDefinedRaptorApplicationFunctionFactory applicationFunctions)
		{
			_authorization = authorization;
			_systemUserAuthorization = new SystemUserAuthorization(applicationFunctions);
		}

		public IDisposable Apply()
		{
			_authorization.OnThisThreadUse(_systemUserAuthorization);
			return new GenericDisposable(() =>
			{
				_authorization.OnThisThreadUse(null);
			});
		}
	}
}