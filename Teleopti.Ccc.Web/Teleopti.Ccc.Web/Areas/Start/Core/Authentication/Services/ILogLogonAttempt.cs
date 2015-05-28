using System;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public interface ILogLogonAttempt
	{
		//TODO: tenant make personId not nullable in db and here and use Guid.Empty instead (that's what happen in new tenant code)
		void SaveAuthenticateResult(string userName, Guid? personId, bool successful);
	}
}