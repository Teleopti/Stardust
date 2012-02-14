using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public static class LicenseActivator
    {
        public static void ProvideLicenseActivator()
        {
            var unitOfWorkFactory = UnitOfWorkFactory.Current;
            IPersonRepository personRepository = new PersonRepository(unitOfWorkFactory);
            var xmlLicenseService = new XmlLicenseService(unitOfWorkFactory, new LicenseRepository(unitOfWorkFactory), personRepository);

            LicenseProvider.ProvideLicenseActivator(xmlLicenseService);
        }
    }
}
