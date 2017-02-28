using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
    public interface IImportForecastsRepository : IRepository<IForecastFile>
    {
    }
}
