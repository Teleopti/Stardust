using System;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class LogLogonAttemptFake : ILogLogonAttempt
	{
		public string UserName;
		public Guid? PersonId;
		public bool Successful;

		public void SaveAuthenticateResult(string userName, Guid? personId, bool successful)
		{
			UserName = userName;
			PersonId = personId;
			Successful = successful;
		}
	}
}