using NUnit.Framework;
using Autofac;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    [TestFixture]
    public class AuthenticationTest
    {
        private ContainerBuilder containerBuilder;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            containerBuilder = new ContainerBuilder();
        }

        [Test]
        public void VerifyProjectionServiceIsCached()
        {
            IApplicationData applicationData = mocks.StrictMock<IApplicationData>();
            ILoadPasswordPolicyService passwordPolicyService = mocks.StrictMock<ILoadPasswordPolicyService>();
            using(mocks.Record())
            {
                Expect.Call(applicationData.LoadPasswordPolicyService).Return(passwordPolicyService);
            applicationData.Dispose();
                LastCall.Repeat.AtLeastOnce();
            }
            using(mocks.Playback())
            {
            containerBuilder.RegisterModule(new AuthenticationModule(applicationData));
            using(var container = containerBuilder.Build())
            {
                var logOnOff = container.Resolve<ILogOnOff>();
                Assert.IsNotNull(logOnOff);

                var policy = container.Resolve<IPasswordPolicy>();
                Assert.IsNotNull(policy);

                var result = container.Resolve<IApplicationData>();
                Assert.IsNotNull(result);
            }
            }
        }
    }
}