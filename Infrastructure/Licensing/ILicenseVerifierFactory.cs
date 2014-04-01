using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	[CLSCompliant(false)]
	public interface ILicenseVerifierFactory
	{
		[CLSCompliant(false)]
		ILicenseVerifier Create(ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory);
	}
}