using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	//shouldn't really be needed
	//but I need some "hook" because of all these
	// "#¤%"#¤"#¤"#¤"#¤ static data
	// same goes for LicenseRepositoryForLicenseVerifier
	public class LicenseVerifierFactory : ILicenseVerifierFactory
	{
		private readonly ILicenseRepositoryForLicenseVerifier _licenseRepository;

		public LicenseVerifierFactory(ILicenseRepositoryForLicenseVerifier licenseRepository)
		{
			_licenseRepository = licenseRepository;
		}

		public ILicenseVerifier Create(
			ILicenseFeedback licenseFeedback, 
			IUnitOfWorkFactory unitOfWorkFactory
			)
		{
			return new LicenseVerifier(
				licenseFeedback,
				unitOfWorkFactory,
				_licenseRepository.MakeRepository(unitOfWorkFactory)
				);
		}
	}

	public interface ILicenseRepositoryForLicenseVerifier
	{
		ILicenseRepository MakeRepository(IUnitOfWorkFactory unitOfWorkFactory);
	}

	public class LicenseRepositoryForLicenseVerifier : ILicenseRepositoryForLicenseVerifier
	{
		public ILicenseRepository MakeRepository(IUnitOfWorkFactory unitOfWorkFactory)
		{
			return LicenseRepository.DONT_USE_CTOR(new FromFactory(() => unitOfWorkFactory));
		}
	}

}