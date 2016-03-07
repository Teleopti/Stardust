using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

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
				_licenseRepository.MakeRepository()
				);
		}
	}

	public interface ILicenseRepositoryForLicenseVerifier
	{
		ILicenseRepository MakeRepository();
	}

	public class LicenseRepositoryForLicenseVerifier : ILicenseRepositoryForLicenseVerifier
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public LicenseRepositoryForLicenseVerifier(IUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public ILicenseRepository MakeRepository()
		{
			return new LicenseRepository(new FromFactory(() => _unitOfWorkFactory));
		}
	}

}