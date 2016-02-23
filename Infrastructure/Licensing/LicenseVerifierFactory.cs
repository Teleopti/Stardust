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
	public class LicenseVerifierFactory : ILicenseVerifierFactory
	{
		private readonly ILicenseRepository _licenseRepository;

		public LicenseVerifierFactory(ILicenseRepository licenseRepository)
		{
			_licenseRepository = licenseRepository;
		}

		public ILicenseVerifier Create(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory)
		{
			return new LicenseVerifier(licenseFeedback, unitOfWorkFactory, _licenseRepository);
		}
	}
}