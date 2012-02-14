using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    //logic tested from other test classes
    [TestFixture]
    public class CacheTest
    {
        private ContainerBuilder containerBuilder;

        [SetUp]
        public void Setup()
        {
            containerBuilder = new ContainerBuilder();
        }


        [Test]
        public void CacheProviderMustBeSet()
        {
            containerBuilder.RegisterModule(new MbCacheModule());
            Assert.Throws<InvalidOperationException>(() => containerBuilder.Build());
        }
    }
}