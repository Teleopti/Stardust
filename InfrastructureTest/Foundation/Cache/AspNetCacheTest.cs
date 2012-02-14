using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation.Cache;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation.Cache
{
    [TestFixture]
    public class AspNetCacheTest : CacheTest
    {
        protected override ICustomDataCache<T> CreateCache<T>()
        {
            return new AspNetCache<T>();
        }

        [Test]
        public void VerifyTimeout()
        {
            Target = new AspNetCache<CacheTestObject>();
            ((AspNetCache<CacheTestObject>) Target).TimeoutMinutes = 3;
            Assert.AreEqual(3, ((AspNetCache<CacheTestObject>)Target).TimeoutMinutes);
        }

    }
}
