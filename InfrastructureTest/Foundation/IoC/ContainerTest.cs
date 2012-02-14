using Autofac;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation.Ioc;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Ioc
{
    [TestFixture]
    public class ContainerTest
    {
        private IContainer container;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            container = mocks.CreateMock<IContainer>();
            Container.Initialize(container);
        }

        [Test]
        public void VerifyInitialize()
        {
            using(mocks.Playback())
            {
                var fromContainer = Container.Instance.Resolve<>()                
            }
        }

        [TearDown]
        public void TearDown()
        {
           Container.Initialize(null);
        }
    }
}
