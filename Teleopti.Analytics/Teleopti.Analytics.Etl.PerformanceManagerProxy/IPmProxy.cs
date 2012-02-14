using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;

namespace Teleopti.Analytics.Etl.PerformanceManagerProxy
{
    public interface IPmProxy : IPMService
    {
        void Abort();
        void Close();
    }
}