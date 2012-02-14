using MbCache.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    public class CacheFactoryTest
    {
        private CacheFactory factory;

        [Test]
        public void VerifyInitialize()
        {
            MockRepository mocks=new MockRepository();
            IMbCacheFactory mbCacheFactory = mocks.CreateMock<IMbCacheFactory>();
            factory = new CacheFactory(mbCacheFactory);    

            Assert.AreSame(mbCacheFactory, factory.MbCacheFactory);
        }

    }
}
