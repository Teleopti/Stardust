using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
	public static class LicenseActivator
	{
		public static void ProvideLicenseActivator()
		{
			//We don't check the agent now, it saves it to database instead
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{

				var xmlLicenseService = new XmlLicenseServiceFactory().Make(new LicenseRepository(uow), 0);

                LicenseProvider.ProvideLicenseActivator(UnitOfWorkFactory.Current.Name,xmlLicenseService);	
			}
		}

	}
}
