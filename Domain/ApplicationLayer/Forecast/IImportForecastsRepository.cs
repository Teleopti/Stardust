using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
    public interface IImportForecastsRepository : IRepository<IForecastFile>
    {
    }
}
