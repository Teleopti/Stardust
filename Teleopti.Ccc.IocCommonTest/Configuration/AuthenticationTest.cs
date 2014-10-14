using NUnit.Framework;
using Autofac;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class AuthenticationTest
	{
		private ContainerBuilder containerBuilder;
		private IApplicationData applicationData;

		[SetUp]
		public void Setup()
		{
			containerBuilder = new ContainerBuilder();
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			containerBuilder.RegisterModule(new CommonModule(){ApplicationData = applicationData}); 
		}

		[Test]
		public void VerifyProjectionServiceIsCached()
		{
			var passwordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			applicationData.Expect(ad => ad.LoadPasswordPolicyService).Return(passwordPolicyService);
			using (var container = containerBuilder.Build())
			{
				var logOnOff = container.Resolve<ILogOnOff>();
				Assert.IsNotNull(logOnOff);

				var policy = container.Resolve<IPasswordPolicy>();
				Assert.IsNotNull(policy);

				var result = container.Resolve<IApplicationData>();
				Assert.IsNotNull(result);
			}
		}

		[Test]
		public void ShouldReturnDummyPasswordPolicyIfNull()
		{
			applicationData.Expect(ad => ad.LoadPasswordPolicyService).Return(null);

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IPasswordPolicy>()
					.Should().Be.InstanceOf<DummyPasswordPolicy>();
			}
		}

		[Test]
		public void ShouldReturnRealPasswordPolicyIfNotNull()
		{
			var policy = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			applicationData.Expect(ad => ad.LoadPasswordPolicyService).Return(policy);

			using (var container = containerBuilder.Build())
			{
				container.Resolve<IPasswordPolicy>()
					.Should().Not.Be.InstanceOf<DummyPasswordPolicy>();
			}
		}
	}
}