using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    [TestFixture]
    public class CacheTest
    {
        [Test]
        public void CacheProviderMustBeSet()
        {
			Assert.Throws<InvalidOperationException>(() => new MbCacheModule(null));
        }
    }
}