using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ImportForecastsRepository: Repository<IForecastFile>, IImportForecastsRepository
    {
        public ImportForecastsRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

				public ImportForecastsRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
	    {
		    
	    }
    }
}
