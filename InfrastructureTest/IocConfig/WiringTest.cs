using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.IocConfig;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.IocConfig
{
    public class WiringTest
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<RuleSetModule>();
            builder.RegisterModule<MbCacheModule>();
            container = builder.Build();
        }

        [Test]
        public void VerifyProjectionServiceIsCached()
        {
            var wsRs = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("f"),
                                                                           new TimePeriodWithSegment(10,10,10,10,10),
                                                                           new TimePeriodWithSegment(10, 10, 10, 10, 10),
                                                                           new ShiftCategory("sdf")));
            var projSvc = container.Resolve<IRuleSetProjectionService>();

            Assert.AreSame(projSvc.ProjectionCollection(wsRs), projSvc.ProjectionCollection(wsRs));
        }


        [TearDown]
        public void Teardown()
        {
            container.Dispose();
        }
    }
}