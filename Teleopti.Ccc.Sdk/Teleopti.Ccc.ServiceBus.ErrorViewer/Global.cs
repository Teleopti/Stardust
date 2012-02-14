using Castle.Windsor;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    public static class Global
    {
        public static void SetContainer(IWindsorContainer container)
        {
            Container = container;
        }

        public static IWindsorContainer Container { get; private set; }
    }
}
