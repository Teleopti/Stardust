using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.Web.Core.Startup.VerifyLicense;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	[TestFixture]
	public class VerifyLicenseTaskTest
	{
		private VerifyLicenseTask target;
		private MockRepository mocks;
		private ILicenseVerifierFactory licenseVerifierFactory;
		private IApplicationData applicationData;
		private ILog logger;

		[SetUp]
		public void Setup()
		{
			mocks=new MockRepository();
			licenseVerifierFactory = mocks.DynamicMock<ILicenseVerifierFactory>();
			logger = mocks.DynamicMock<ILog>();
			applicationData = mocks.DynamicMock<IApplicationData>();
			target = new VerifyLicenseTask(licenseVerifierFactory, new Lazy<IApplicationData>(() => applicationData), logger);
			DefinedLicenseDataFactory.SetLicenseActivator("asdf", null);
		}

		[Test]
		public void ShouldSetLicenseActivatorOnHappyPath()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var dataSources = new List<IDataSource> {dataSource};
			var licenseVerifier = mocks.DynamicMock<ILicenseVerifier>();

			using(mocks.Record())
			{
			    Expect.Call(dataSource.DataSourceName).Return("asdf");
				Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
				Expect.Call(licenseVerifierFactory.Create(target, null)).Return(licenseVerifier);
				Expect.Call(licenseVerifier.LoadAndVerifyLicense()).Return(mocks.DynamicMock<ILicenseService>());
			}
			using(mocks.Playback())
			{
				target.Execute();
				DefinedLicenseDataFactory.GetLicenseActivator("asdf")
					.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldNotSetLicenseActivatorOnUnHappyPath()
		{
			var dataSource = mocks.DynamicMock<IDataSource>();
			var dataSources = new List<IDataSource> { dataSource };
			var uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			var licenseVerifier = new LicenseVerifier(target, uowFactory, null);

			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection).Return(dataSources);
				Expect.Call(licenseVerifierFactory.Create(target, null)).Return(licenseVerifier);
				//just to fake a license exception
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Throw(new LicenseMissingException());
			}
			using (mocks.Playback())
			{
				Assert.Throws<PermissionException>(() => target.Execute());
				DefinedLicenseDataFactory.GetLicenseActivator("asdf")
					.Should().Be.Null();
			}
		}

		[Test]
		public void ErrorAndWarningIsLogged()
		{
			using(mocks.Record())
			{
				logger.Warn("Should be");
				logger.Error("logged");
			}
			using(mocks.Playback())
			{
				target.Warning("Should be");
				Assert.Throws<PermissionException>(() => target.Error("logged"));
			}
		}
	}
}