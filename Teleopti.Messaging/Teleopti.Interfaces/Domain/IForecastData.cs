namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface used by forecasting entities (primarily for listening to any changes in forecast via message broker)
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-09-24
    /// </remarks>
    public interface IForecastData : IAggregateRoot,
                                        IChangeInfo
    {
    }
}
