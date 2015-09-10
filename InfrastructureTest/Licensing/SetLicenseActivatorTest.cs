using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	public class SetLicenseActivatorTest
	{
		private const string datasourceName = "hejsvejs";

		[SetUp]
		public void Setup()
		{
			DefinedLicenseDataFactory.SetLicenseActivator(datasourceName, null);
		}

		[Test]
		public void ShouldSetLicenseActivatorOnHappyPath()
		{
			var dataSource = new FakeDataSource(datasourceName);
			var licenseVerifierFactory = MockRepository.GenerateStub<ILicenseVerifierFactory>();
			var target = new SetLicenseActivator(null, licenseVerifierFactory);
			var licenseVerifier = MockRepository.GenerateStub<ILicenseVerifier>();

			licenseVerifierFactory.Expect(x => x.Create(target, null)).Return(licenseVerifier);
			licenseVerifier.Expect(x => x.LoadAndVerifyLicense()).Return(MockRepository.GenerateStub<ILicenseService>());

			target.Execute(dataSource);
			DefinedLicenseDataFactory.GetLicenseActivator(datasourceName)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotSetLicenseActivatorOnUnHappyPath()
		{
			var dataSource = new FakeDataSource(datasourceName);
			var licenseVerifierFactory = MockRepository.GenerateStub<ILicenseVerifierFactory>();
			var target = new SetLicenseActivator(null, licenseVerifierFactory);
			var licenseVerifier = MockRepository.GenerateStub<ILicenseVerifier>();

			licenseVerifierFactory.Expect(x => x.Create(target, null)).Return(licenseVerifier);
			licenseVerifier.Expect(x => x.LoadAndVerifyLicense()).Return(null);

			target.Execute(dataSource);
			DefinedLicenseDataFactory.GetLicenseActivator(datasourceName)
				.Should().Be.Null();

		}

		[Test]
		public void ErrorAndWarningIsLogged()
		{
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new SetLicenseActivator(logger, MockRepository.GenerateStub<ILicenseVerifierFactory>());

			target.Warning("should be");
			target.Error("logged");

			logger.AssertWasCalled(x => x.Warn("should be"));
			logger.AssertWasCalled(x => x.Error("logged"));
		}
	}
}