using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ImportForecastsRepository: Repository<IForecastFile>, IImportForecastsRepository
    {
		public static ImportForecastsRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ImportForecastsRepository(currentUnitOfWork, null, null);
		}

		public ImportForecastsRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
					: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
	    {
	    }
    }
}
