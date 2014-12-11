using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    [TestFixture]
    public class ScheduleScreenRefresherModuleTest
    {
        private ContainerBuilder _containerBuilder;
        private ScheduleScreenRefresherModule _module;
        private IContainer _container;

        [SetUp]
        public void Setup()
        {
            _module = new ScheduleScreenRefresherModule();
			_containerBuilder = new ContainerBuilder();
			_containerBuilder.RegisterModule(CommonModule.ForTest());
			_containerBuilder.RegisterModule(_module);
            _container = _containerBuilder.Build();
        }

        [Test]
        public void ShouldResolveScheduleScreenRefresherAsFarAsPossible()
        {
            var mocks = new MockRepository();

            var instance = _container.Resolve<IScheduleScreenRefresher>(
                TypedParameter.From(mocks.Stub<IReassociateDataForSchedules>()),
                TypedParameter.From(_container.Resolve<IScheduleRefresher>(
                    TypedParameter.From(mocks.Stub<IUpdateScheduleDataFromMessages>()),
										TypedParameter.From(mocks.Stub<IMessageQueueRemoval>())
                    )),
                TypedParameter.From(_container.Resolve<IScheduleDataRefresher>(
										TypedParameter.From(mocks.Stub<IUpdateScheduleDataFromMessages>()),
										TypedParameter.From(mocks.Stub<IMessageQueueRemoval>())
                    )),
                TypedParameter.From(_container.Resolve<IMeetingRefresher>(
										TypedParameter.From(mocks.Stub<IUpdateMeetingsFromMessages>()),
										TypedParameter.From(mocks.Stub<IMessageQueueRemoval>())
                    )),
                TypedParameter.From(_container.Resolve<IPersonRequestRefresher>(
										TypedParameter.From(mocks.Stub<IUpdatePersonRequestsFromMessages>()),
										TypedParameter.From(mocks.Stub<IMessageQueueRemoval>())
                    ))
                );

            Assert.That(instance, Is.InstanceOf<ScheduleScreenRefresher>());
        }
    }
}