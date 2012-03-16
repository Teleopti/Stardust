﻿using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class LicenseActivator
    {
        public static void ProvideLicenseActivator()
        {
            //We don't check the agent now, it saves it to database instead
            var xmlLicenseService = new XmlLicenseService(new LicenseRepository(UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork()), 0);

            LicenseProvider.ProvideLicenseActivator(xmlLicenseService);
        }
    }
}
