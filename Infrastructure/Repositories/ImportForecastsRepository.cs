using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ImportForecastsRepository: Repository<IForecastFile>, IImportForecastsRepository
    {
        public ImportForecastsRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

				public ImportForecastsRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
    }
}
