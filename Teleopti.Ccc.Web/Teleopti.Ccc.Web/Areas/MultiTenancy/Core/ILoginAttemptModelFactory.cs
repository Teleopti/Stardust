using System;
using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface ILoginAttemptModelFactory
	{
		LoginAttemptModel Create(string userName, Guid? personId, bool wasSuccesful);
	}
}