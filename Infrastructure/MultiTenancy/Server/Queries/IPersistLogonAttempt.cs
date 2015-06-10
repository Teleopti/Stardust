using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistLogonAttempt
	{
		void SaveLoginAttempt(LoginAttemptModel model);
	}
}