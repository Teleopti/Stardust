using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
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

        public IForecastFile LoadForecastFileFromDb(Guid id)
        {
            var forecastFile = Session.CreateCriteria<ForecastFile>()
                .Add(Restrictions.Eq("Id", id))
                .List<IForecastFile>();

            var forecastFileData = forecastFile.FirstOrDefault();
            return forecastFileData;
        }

    }
}
