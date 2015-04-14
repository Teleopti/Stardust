using System;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface ILogLogonAttempt
	{
		void SaveAuthenticateResult(string userName, Guid? personId, bool successful);
	}
}