using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class NoLicenseServiceInitialization : IInitializeLicenseServiceForTenant
	{
		public void Warning(string warning)
		{
		}

		public void Warning(string warning, string caption)
		{
		}

		public void Error(string error)
		{
		}

		public bool TryInitialize(IDataSource dataSource) => false;
	}
}