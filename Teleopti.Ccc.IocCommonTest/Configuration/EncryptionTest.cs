using NUnit.Framework;
using Autofac;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    public class EncryptionTest
    {
        private ContainerBuilder containerBuilder;

        [SetUp]
        public void Setup()
        {
            containerBuilder = new ContainerBuilder();
        }


        [Test]
        public void VerifyProjectionServiceIsCached()
        {
            containerBuilder.RegisterModule(new EncryptionModule());
            using(var container = containerBuilder.Build())
            {
                var encryption = container.Resolve<IOneWayEncryption>();

                Assert.IsNotNull(encryption);
            }            
        }
    }
}