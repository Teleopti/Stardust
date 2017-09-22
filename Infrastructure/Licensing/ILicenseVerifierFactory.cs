using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public interface ILicenseVerifierFactory
	{
		ILicenseVerifier Create(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory);
	}
}