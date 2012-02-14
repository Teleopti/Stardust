namespace Teleopti.Analytics.Etl.PerformanceManagerProxy
{
    public class PmProxyFactory : IPmProxyFactory
    {
        public IPmProxy CreateProxy()
        {
            return new PmProxy();
        }
    }
}
