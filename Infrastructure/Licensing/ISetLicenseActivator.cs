using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ISetLicenseActivator : ILicenseFeedback
	{
		void Execute(IUnitOfWorkFactory unitOfWorkFactory);
	}
}