using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IPersistLogonAttempt
	{
		void SaveLoginAttempt(LoginAttemptModel model);
	}
}