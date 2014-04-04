using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	//shouldn't really be needed
	//but I need some "hook" because of all these
	// "#¤%"#¤"#¤"#¤"#¤ static data
	public class LicenseVerifierFactory : ILicenseVerifierFactory
	{
		public ILicenseVerifier Create(ILicenseFeedback licenseFeedback, 
										IUnitOfWorkFactory unitOfWorkFactory)  
		{
			return new LicenseVerifier(licenseFeedback, unitOfWorkFactory,
																  new LicenseRepository(unitOfWorkFactory));
		}
	}
}