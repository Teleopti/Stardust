using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Main
{
    public interface ILogonLicenseChecker
    {
        bool HasValidLicense(IDataSourceContainer dataSourceContainer);
    }

    public class LogonLicenseChecker : ILogonLicenseChecker
    {
        private readonly ILicenseFeedback _view;
        private readonly ILicenseStatusLoader _licenseStatusLoader;

        private readonly ILicenseVerifierFactory _licenseVerifierFactory;

        public LogonLicenseChecker(ILicenseFeedback view, ILicenseStatusLoader licenseStatusLoader, ILicenseVerifierFactory licenseVerifierFactory)
        {
            _view = view;
            _licenseStatusLoader = licenseStatusLoader;
            _licenseVerifierFactory = licenseVerifierFactory;
        }

        public bool HasValidLicense(IDataSourceContainer dataSourceContainer)
        {
            return initializeLicense(dataSourceContainer.DataSource.Application);
        }

        private bool initializeLicense(IUnitOfWorkFactory unitOfWorkFactory)
        {
            using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var verifier = _licenseVerifierFactory.Create(_view, unitOfWorkFactory);
                var licenseService = verifier.LoadAndVerifyLicense();
                if (licenseService == null) return false;
                LicenseProvider.ProvideLicenseActivator(unitOfWorkFactory.Name, licenseService);
                return checkStatusOfLicense(licenseService, uow ,unitOfWorkFactory.Name);
            }
        }

        private bool checkStatusOfLicense(ILicenseService licenseService,IUnitOfWork uow , string dataSourceName)
        {
            var licenseStatus = _licenseStatusLoader.GetStatus(uow);
            
            if (licenseStatus.StatusOk && licenseStatus.AlmostTooMany)
            {
                _view.Warning(getAlmostTooManyAgentsWarning(licenseStatus.NumberOfActiveAgents, licenseService));
            }
            if (!licenseStatus.StatusOk && licenseStatus.DaysLeft >= 0)
            {
                _view.Warning(getLicenseIsOverUsedWarning(licenseService, licenseStatus));
            }
            if (!licenseStatus.StatusOk && licenseStatus.DaysLeft < 0)
            {
                _view.Error(getTooManyAgentsExplanation(licenseService, dataSourceName,
                                                  licenseStatus.NumberOfActiveAgents));
                return false;
            }
        
            return true;
        }

        private static string getAlmostTooManyAgentsWarning(int numberOfActiveAgents, ILicenseService licenseService)
        {
            return licenseService.LicenseType.Equals(LicenseType.Agent)
                       ? String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManyActiveAgents,
                                       numberOfActiveAgents, licenseService.MaxActiveAgents)
                       : String.Format(CultureInfo.CurrentCulture, Resources.YouHaveAlmostTooManySeats,
                                       licenseService.MaxSeats);
        }

        private static string getLicenseIsOverUsedWarning(ILicenseService licenseService, ILicenseStatusXml licenseStatus)
        {
            var maxLicensed = licenseService.LicenseType.Equals(LicenseType.Agent)
                                  ? licenseService.MaxActiveAgents
                                  : licenseService.MaxSeats;

            return String.Format(CultureInfo.CurrentCulture, Resources.TooManyAgentsIsUsedWarning,
                                 licenseStatus.NumberOfActiveAgents, maxLicensed, licenseStatus.DaysLeft);
        }

        private static string getTooManyAgentsExplanation(ILicenseService licenseService, string dataSourceName,
                                                          int numberOfActiveAgents)
        {
            return licenseService.LicenseType == LicenseType.Agent
                       ? dataSourceName + "\r\n" +
                         String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManyActiveAgents,
                                       numberOfActiveAgents, licenseService.MaxActiveAgents)
                       : dataSourceName + "\r\n" + String.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats,
                                                                 licenseService.MaxSeats);
        }

        
    }
}