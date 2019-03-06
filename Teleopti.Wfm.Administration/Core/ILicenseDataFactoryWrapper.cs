using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Wfm.Administration.Core
{
	public interface ILicenseDataFactoryWrapper
	{
		ILicenseActivator GetLicenseActivator(string tenantName);
	}
}