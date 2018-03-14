using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public class SetNoLicenseActivator : ISetLicenseActivator
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

		public void Execute(IUnitOfWorkFactory unitOfWorkFactory)
		{
		}
	}
}