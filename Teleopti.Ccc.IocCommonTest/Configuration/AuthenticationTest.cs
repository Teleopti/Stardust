using NUnit.Framework;
using Autofac;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	public class AuthenticationTest
	{
		private ContainerBuilder builder;
		private IApplicationData applicationData;

		[SetUp]
		public void Setup()
		{
			builder = new ContainerBuilder();
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			builder.RegisterModule(CommonModule.ForTest());
			builder.RegisterInstance(applicationData);
		}

		[Test]
		public void VerifyProjectionServiceIsCached()
		{
			var passwordPolicyService = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			applicationData.Expect(ad => ad.LoadPasswordPolicyService).Return(passwordPolicyService);
			using (var container = builder.Build())
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

			using (var container = builder.Build())
			{
				container.Resolve<IPasswordPolicy>()
					.Should().Be.InstanceOf<PasswordPolicyFake>();
			}
		}

		[Test]
		public void ShouldReturnRealPasswordPolicyIfNotNull()
		{
			var policy = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			applicationData.Expect(ad => ad.LoadPasswordPolicyService).Return(policy);

			using (var container = builder.Build())
			{
				container.Resolve<IPasswordPolicy>()
					.Should().Not.Be.InstanceOf<PasswordPolicyFake>();
			}
		}
	}
}