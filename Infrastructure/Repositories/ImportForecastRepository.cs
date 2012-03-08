using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ImportForecastRepository: Repository<IForecastFile>
    {
        public ImportForecastRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public ImportForecastRepository(IUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }

    }
}
