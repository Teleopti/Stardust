using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public interface IPersistLogonAttempt
	{
		void SaveLoginAttempt(LoginAttemptModel model);
	}
}