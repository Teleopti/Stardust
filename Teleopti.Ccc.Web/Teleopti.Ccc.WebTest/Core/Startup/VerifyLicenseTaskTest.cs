using System;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.Web.Core.Startup.VerifyLicense;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class VerifyLicenseTaskTest
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
			var dataSource = MockRepository.GenerateStub<IDataSource>();
			var licenseVerifierFactory = MockRepository.GenerateStub<ILicenseVerifierFactory>();
			var dsForTenant = new DataSourceForTenant(null);
			dsForTenant.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);
			var target = new VerifyLicenseTask(licenseVerifierFactory, new Lazy<IDataSourceForTenant>(
					() => dsForTenant), null);
			var licenseVerifier = MockRepository.GenerateStub<ILicenseVerifier>();

			dataSource.Expect(x => x.DataSourceName).Return(datasourceName);
			licenseVerifierFactory.Expect(x => x.Create(target, null)).Return(licenseVerifier);
			licenseVerifier.Expect(x => x.LoadAndVerifyLicense()).Return(MockRepository.GenerateStub<ILicenseService>());

			target.Execute(null);
			DefinedLicenseDataFactory.GetLicenseActivator(datasourceName)
				.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotSetLicenseActivatorOnUnHappyPath()
		{
			var dataSource = MockRepository.GenerateStub<IDataSource>();
			var uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
			var licenseVerifierFactory = MockRepository.GenerateStub<ILicenseVerifierFactory>();
			var dsForTenant = new DataSourceForTenant(null);
			dsForTenant.MakeSureDataSourceExists_UseOnlyFromTests(dataSource);
			var target = new VerifyLicenseTask(licenseVerifierFactory, new Lazy<IDataSourceForTenant>(
					() => dsForTenant), MockRepository.GenerateMock<ILog>());
			var licenseVerifier = new LicenseVerifier(target, uowFactory, null);

			dataSource.Expect(x => x.DataSourceName).Return(datasourceName);
			licenseVerifierFactory.Expect(x => x.Create(target, null)).Return(licenseVerifier);
			//just to fake a license exception
			uowFactory.Expect(x => x.CreateAndOpenUnitOfWork()).Throw(new LicenseMissingException());
		
			target.Execute(null);
			DefinedLicenseDataFactory.GetLicenseActivator(datasourceName)
					.Should().Be.Null();

		}

		[Test]
		public void ErrorAndWarningIsLogged()
		{
			var logger = MockRepository.GenerateMock<ILog>();
			var target = new VerifyLicenseTask(MockRepository.GenerateStub<ILicenseVerifierFactory>(), new Lazy<IDataSourceForTenant>(
					() => new DataSourceForTenant(null)), logger);

			target.Warning("should be");
			target.Error("logged");

			logger.AssertWasCalled(x => x.Warn("should be"));
			logger.AssertWasCalled(x => x.Error("logged"));
		}
	}
}