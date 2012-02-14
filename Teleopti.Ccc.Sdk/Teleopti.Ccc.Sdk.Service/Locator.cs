using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.Sdk.WcfService
{
    //rk: dont like this one, but don't know how to inject something to a wcf service... Robin?
    public class Locator
    {
        public static void Initialize(IIocContainer container)
        {
            Container = container;
        }

        public static IIocContainer Container { get; private set; }
    }
}