namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public interface ILicenseActivatorProvider
	{
		ILicenseActivator Current();
	}
}