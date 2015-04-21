namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface ICheckPasswordStrength
	{
		void Validate(string newPassword);
	}
}