using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Autofac;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

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
        public void ShouldResolveDefaultHashFunctionWhenUsingOldToggle()
        {
            containerBuilder.RegisterModule(new EncryptionModule(new IocConfiguration(new IocArgs(new FakeConfigReader()), new FalseToggleManager())));
            using(var container = containerBuilder.Build())
            {
                var hashFunction = container.Resolve<IHashFunction>();

                Assert.IsNotNull(hashFunction);
				Assert.True(hashFunction is OneWayEncryption);
            }            
        }

		[Test]
		public void ShouldResolveDefaultHashFunctionWhenUsingNewToggle()
		{
			containerBuilder.RegisterModule(new EncryptionModule(new IocConfiguration(new IocArgs(new FakeConfigReader()), new TrueToggleManager())));
			using (var container = containerBuilder.Build())
			{
				var hashFunction = container.Resolve<IHashFunction>();

				Assert.IsNotNull(hashFunction);
				Assert.True(hashFunction is BCryptHashFunction);
			}
		}

		[Test]
	    public void ShouldResolveAllHashFunctions()
	    {
			containerBuilder.RegisterModule(new EncryptionModule(new IocConfiguration(new IocArgs(new FakeConfigReader()), new FakeToggleManager())));
			using (var container = containerBuilder.Build())
			{
				var hashFunctions = container.Resolve<IEnumerable<IHashFunction>>().ToList();

				Assert.IsNotEmpty(hashFunctions);
				hashFunctions.Count.Should().Be.GreaterThan(1);
			}
		}
    }
}