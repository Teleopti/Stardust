using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class LicenseFactory : ILicenseFeedback
    {
        private readonly ILicenseCache _licenseCache;
        private LicenseVerificationResultDto _licenseVerificationResultDto;

        public LicenseFactory(ILicenseCache licenseCache)
        {
            _licenseCache = licenseCache;
        }

        /// <summary>
        /// Verifies the license.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 12/9/2008
        /// </remarks>
        public LicenseVerificationResultDto VerifyLicense(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _licenseVerificationResultDto = _licenseCache.Get();
            if (_licenseVerificationResultDto == null)
            {
                _licenseVerificationResultDto = new LicenseVerificationResultDto(false);
								var verifier = new LicenseVerifier(this, unitOfWorkFactory, new LicenseRepository(unitOfWorkFactory));
                var licenseService = verifier.LoadAndVerifyLicense();

                if (licenseService != null)
                {
                    _licenseVerificationResultDto.SetValidLicenseFoundTrue();
                    LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);
                    _licenseVerificationResultDto.LicenseHolderName =
                        DefinedLicenseDataFactory.GetLicenseActivator(unitOfWorkFactory.Name).CustomerName;
                }

                _licenseCache.Add(_licenseVerificationResultDto);
            }
            return _licenseVerificationResultDto;
        }

        public void Warning(string warning)
        {
            Warning(warning, "");
        }

        public void Warning(string warning, string caption)
        {
            _licenseVerificationResultDto.IsWarningFound = true;

            FaultDto exception = new FaultDto(warning);
            _licenseVerificationResultDto.AddWarningToCollection(exception);
        }

        public void Error(string error)
        {
            _licenseVerificationResultDto.IsExceptionFound = true;

            FaultDto exception = new FaultDto(error);
            _licenseVerificationResultDto.AddExceptionToCollection(exception);
        }
    }
}