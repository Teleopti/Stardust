using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IForecastFile : IAggregateRoot, IChangeInfo
    {
        string FileName { get; }
        byte[] FileContent { get; }
    }
}
