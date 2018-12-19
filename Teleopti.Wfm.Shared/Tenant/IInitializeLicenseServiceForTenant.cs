using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface IInitializeLicenseServiceForTenant : ILicenseFeedback
	{
		bool TryInitialize(IDataSource dataSource);
	}
}