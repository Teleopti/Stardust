using System;
using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface ILoginAttemptModelFactory
	{
		LoginAttemptModel Create(string userName, Guid? personId, bool wasSuccesful);
	}
}