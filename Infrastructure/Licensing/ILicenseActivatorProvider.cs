using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseActivatorProvider
	{
		ILicenseActivator Current();
	}
}