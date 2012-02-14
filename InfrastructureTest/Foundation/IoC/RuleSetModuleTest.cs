using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Ioc
{
    [TestFixture]
    public class RuleSetModuleTest
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new RuleSetModule());
            builder.RegisterModule(new CacheModule());
            container = builder.Build();
        }

        [Test]
        public void CanGetRuleSetProjectionService()
        {
            var svc = container.Resolve<IRuleSetProjectionService>();
            Assert.IsInstanceOf<RuleSetProjectionServiceCache>(svc);
        }

        [Test]
        public void VerifyRuleSetProjectionServiceAreSingleton()
        {
            Assert.AreSame(container.Resolve<IRuleSetProjectionService>(), container.Resolve<IRuleSetProjectionService>());
        }
    }
}