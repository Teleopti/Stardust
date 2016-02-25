using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	//shouldn't really be needed
	//but I need some "hook" because of all these
	// "#¤%"#¤"#¤"#¤"#¤ static data
	public class LicenseVerifierFactory : ILicenseVerifierFactory
	{
		public ILicenseVerifier Create(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory)
		{
			// cant inject this yet!
			var licenseRepository = new LicenseRepository(new FromFactory(() => unitOfWorkFactory));
			return new LicenseVerifier(licenseFeedback, unitOfWorkFactory, licenseRepository);
		}
	}

	// for testing, with injected repository
	public class FakeLicenseVerifierFactory : ILicenseVerifierFactory
	{
		private readonly ILicenseRepository _licenseRepository;

		public FakeLicenseVerifierFactory(ILicenseRepository licenseRepository)
		{
			_licenseRepository = licenseRepository;
		}

		public ILicenseVerifier Create(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new LicenseVerifier(licenseFeedback, unitOfWorkFactory, _licenseRepository);
		}
	}
}