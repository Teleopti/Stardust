using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;


namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
	[TestFixture]
	[Category("BucketB")]
	public class XmlLicenseServiceTest : IDisposable
	{
		private ILicenseService _licenseService;
		private MockRepository _mocks;
		private const string folder = "Licensing\\";
		private const string unsignedLicenseFileName = folder + "TeleoptiCCCUnsignedLicense.xml";
		private const string unsignedSeatLicenseFileName = folder + "TeleoptiCCCUnsignedSeatLicense.xml";
		private const string unsignedNoMajorLicenseFileName = folder + "TeleoptiCCCNoMajorUnsignedLicense.xml";
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		}

		[Test]
		public void VerifyConstructor()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			ILicense license = new License { XmlString = doc.OuterXml };

			var unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

			using (_mocks.Record())
			{
				Expect.Call(licenseRepository.LoadAll()).Return(new List<ILicense> { license }).Repeat.Once();
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			Assert.Throws<SignatureValidationException>(() =>
			{
				using (_mocks.Playback())
				{
					const int numberOfActiveAgents = 43289;

					_licenseService = new XmlLicenseServiceFactory().Make(licenseRepository, numberOfActiveAgents);
				}
			});
		}

		[Test]
		public void ShouldCreateInstanceWithUnitOfWorkFactory()
		{
			var unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var personRepository = _mocks.StrictMock<IPersonRepository>();

			using (_mocks.Record())
			{
				Expect.Call(personRepository.NumberOfActiveAgents()).Return(10);
				Expect.Call(licenseRepository.LoadAll()).Return(new List<ILicense>());
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			Assert.Throws<LicenseMissingException>(() =>
			{
				using (_mocks.Playback())
				{
					_licenseService = new XmlLicenseServiceFactory().Make(unitOfWorkFactory, licenseRepository, personRepository);
				}
			});
		}

		[Test]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void VerifyExceptionInConstructor()
		{
			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();

			using (_mocks.Record())
			{
				Expect.Call(licenseRepository.LoadAll()).Return(new List<ILicense>());
			}
			Assert.Throws<LicenseMissingException>(() =>
			{
				using (_mocks.Playback())
				{
					_licenseService = new XmlLicenseServiceFactory().Make(licenseRepository, 4329);
				}
			});
		}

		[Test]
		public void VerifyCreationOfValidLicense()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);
			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));

			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var reader = new XmlNodeReader(doc))
			{
				_licenseService = new XmlLicenseService(XDocument.Load(reader), XmlLicenseTestSetupFixture.PublicKeyXmlString, 10);
			}

			Assert.AreEqual("This license is stolen!", _licenseService.CustomerName);
			Assert.AreEqual(new DateTime(2025, 02, 02, 12, 0, 0), _licenseService.ExpirationDate);
			Assert.AreEqual(new TimeSpan(30, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.AreEqual(10000, _licenseService.MaxActiveAgents);
			Assert.AreEqual(new Percent(0.1).Value, _licenseService.MaxActiveAgentsGrace);

			Assert.IsTrue(_licenseService.TeleoptiCccBaseEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccShiftTradesEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccHolidayPlannerEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccPerformanceManagerEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
			Assert.IsTrue(_licenseService.TeleoptiCccMyTimeWebEnabled);
			
			Assert.IsFalse(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
		}

		/// <summary>
		/// test pilot license. This test assumes that signing key is in standard container
		/// </summary>
		[Test]
		public void VerifyCreationOfPilotCustomerLicense()
		{
			const string unsignedPilotCustomersLicenseFileName = folder + "TeleoptiCCCPilotCustomersUnsignedLicense.xml";

			XmlDocument doc = new XmlDocument();
			doc.Load(unsignedPilotCustomersLicenseFileName);
			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var reader = new XmlNodeReader(doc))
			{
				_licenseService = new XmlLicenseService(XDocument.Load(reader),
														XmlLicenseTestSetupFixture.PublicKeyXmlString, 100);
			}

			Assert.AreEqual("This pilot license is stolen!", _licenseService.CustomerName);
			Assert.GreaterOrEqual(new TimeSpan(31, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.LessOrEqual(new TimeSpan(28, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.AreEqual(1000, _licenseService.MaxActiveAgents);
			Assert.AreEqual(new Percent(1).Value, _licenseService.MaxActiveAgentsGrace);

			Assert.IsFalse(_licenseService.TeleoptiCccBaseEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccShiftTradesEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccHolidayPlannerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccPerformanceManagerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccMyTimeWebEnabled);
			
			Assert.IsFalse(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
		}

		/// <summary>
		/// test freemium license. This test assumes that signing key is in standard container
		/// </summary>
		[Test]
		public void VerifyCreationOfFreemiumLicense()
		{
			const string unsignedFreemiumLicenseFileName = folder + "TeleoptiCCCFreemiumUnsignedLicense.xml";

			XmlDocument doc = new XmlDocument();
			doc.Load(unsignedFreemiumLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var reader = new XmlNodeReader(doc))
			{
				XDocument licenseXml = XDocument.Load(reader);
				_licenseService = new XmlLicenseService(licenseXml, XmlLicenseTestSetupFixture.PublicKeyXmlString, 1);
			}

			Assert.AreEqual("This freemium license is stolen!", _licenseService.CustomerName);
			Assert.AreEqual(new TimeSpan(0, 20, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.AreEqual(1, _licenseService.MaxActiveAgents);
			Assert.AreEqual(new Percent(0.2).Value, _licenseService.MaxActiveAgentsGrace);

			Assert.IsFalse(_licenseService.TeleoptiCccBaseEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccShiftTradesEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccHolidayPlannerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccPerformanceManagerEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
			Assert.IsFalse(_licenseService.TeleoptiCccMyTimeWebEnabled);

			Assert.IsTrue(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
		}

		[Test]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void VerifyLicenseFileMissingHandling()
		{
			Assert.Throws<LicenseMissingException>(() => new XmlLicenseService(null, XmlLicenseTestSetupFixture.PublicKeyXmlString, 432));
		}


		[Test]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void VerifyLicenseExpired()
		{
			const string unsignedExpiredLicenseFileName = folder + "TeleoptiCCCExpiredUnsignedLicense.xml";

			XmlDocument doc = new XmlDocument();
			doc.Load(unsignedExpiredLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			Assert.Throws<LicenseExpiredException>(() => new XmlLicenseService(XDocument.Load(new XmlNodeReader(doc)), XmlLicenseTestSetupFixture.PublicKeyXmlString, 234));
		}

		[Test]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void VerifySave()
		{
			string licenseFileName = Path.GetTempFileName();

			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var personRepository = _mocks.StrictMock<IPersonRepository>();
			const int numberOfActiveAgents = 100;

			using (var writer = new XmlTextWriter(licenseFileName, System.Text.Encoding.Unicode))
			{
				doc.WriteTo(writer);
				writer.Flush();
			}

			using (_mocks.Record())
			{
				Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
				licenseRepository.Add(new License());
				LastCall.IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				ILicenseService licenseService =
					 new XmlLicensePersister().SaveNewLicense(licenseFileName, licenseRepository,
																 XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
				Assert.AreEqual(10000, licenseService.MaxActiveAgents);
				File.Delete(licenseFileName);
			}
		}

		[Test]
		public void VerifySave2()
		{
			string licenseFileName = Path.GetTempFileName();

			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var writer = new XmlTextWriter(licenseFileName, System.Text.Encoding.Unicode))
			{
				doc.WriteTo(writer);
				writer.Flush();
			}

			var uow = _mocks.DynamicMock<IUnitOfWork>();
			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var personRepository = _mocks.StrictMock<IPersonRepository>();
			var unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

			const int numberOfActiveAgents = 100;

			using (_mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.Name).Return("asdf");
				Expect.Call(unitOfWorkFactory.CurrentUnitOfWork()).Return(uow);
				Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
				Expect.Call(uow.PersistAll()).Return(null);

				licenseRepository.Add(new License());
				LastCall.IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				new XmlLicensePersister().SaveNewLicense(licenseFileName, unitOfWorkFactory, licenseRepository, XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
				File.Delete(licenseFileName);
			}
		}

		[Test]
		public void VerifySaveThrowsExceptionWhenHibernateError()
		{
			string licenseFileName = Path.GetTempFileName();

			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var writer = new XmlTextWriter(licenseFileName, System.Text.Encoding.Unicode))
			{
				doc.WriteTo(writer);
				writer.Flush();
			}

			var uow = _mocks.DynamicMock<IUnitOfWork>();
			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var personRepository = _mocks.StrictMock<IPersonRepository>();
			var unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();

			const int numberOfActiveAgents = 100;

			using (_mocks.Record())
			{
				Expect.Call(unitOfWorkFactory.Name).Return("asdf");
				Expect.Call(unitOfWorkFactory.CurrentUnitOfWork()).Return(uow);
				Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
				Expect.Call(uow.PersistAll()).Throw(new HibernateException());

				licenseRepository.Add(new License());
				LastCall.IgnoreArguments();
			}
			Assert.Throws<DataSourceException>(() =>
			{
				using (_mocks.Playback())
				{
					new XmlLicensePersister().SaveNewLicense(licenseFileName, unitOfWorkFactory, licenseRepository,
																XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
				}
			});
		}

		[Test]
		public void VerifySaveFailsIfTooManyActiveAgents()
		{
			string licenseFileName = Path.GetTempFileName();

			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var writer = new XmlTextWriter(licenseFileName, System.Text.Encoding.Unicode))
			{
				doc.WriteTo(writer);
				writer.Flush();
			}

			var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
			var personRepository = _mocks.StrictMock<IPersonRepository>();
			Expect.Call(personRepository.NumberOfActiveAgents()).Return(20000);

			licenseRepository.Add(new License());
			LastCall.IgnoreArguments();

			_mocks.ReplayAll();
			Assert.Throws<TooManyActiveAgentsException>(
				() => new XmlLicensePersister().SaveNewLicense(licenseFileName, licenseRepository, XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository));
		}

		[Test]
		public void VerifyActiveAgentsCheck()
		{
			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var reader = new XmlNodeReader(doc))
			{
				_licenseService = new XmlLicenseService(XDocument.Load(reader), XmlLicenseTestSetupFixture.PublicKeyXmlString, 100);
			}

			Assert.IsTrue(_licenseService.IsThisTooManyActiveAgents(11100));
			Assert.IsTrue(_licenseService.IsThisAlmostTooManyActiveAgents(10100));
		}

		[Test]
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void VerifyActiveAgentsException()
		{
			var doc = new XmlDocument();
			doc.Load(unsignedLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			Assert.Throws<TooManyActiveAgentsException>(() => new XmlLicenseService(XDocument.Load(new XmlNodeReader(doc)), XmlLicenseTestSetupFixture.PublicKeyXmlString, 11100));
		}

		[Test]
		public void ShouldRecalculateMaxAgentsIfSeatType()
		{
			var doc = new XmlDocument();
			doc.Load(unsignedSeatLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			using (var reader = new XmlNodeReader(doc))
			{
				_licenseService = new XmlLicenseService(XDocument.Load(reader), XmlLicenseTestSetupFixture.PublicKeyXmlString, 1500);
			}

			Assert.AreEqual("This license is stolen!", _licenseService.CustomerName);
			Assert.AreEqual(new DateTime(2025, 02, 02, 12, 0, 0), _licenseService.ExpirationDate);
			Assert.AreEqual(new TimeSpan(30, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.AreEqual(new Percent(0.1).Value, _licenseService.MaxActiveAgentsGrace);

			Assert.That(_licenseService.LicenseType, Is.EqualTo(LicenseType.Seat));
			Assert.That(_licenseService.MaxSeats, Is.EqualTo(1000));
			Assert.That(_licenseService.Ratio, Is.EqualTo(new decimal(2)));
			Assert.That(_licenseService.MaxActiveAgents, Is.EqualTo(2000));
		}

		[Test]
		public void ShouldThrowIfNoMajorVersion()
		{
			var doc = new XmlDocument();
			doc.Load(unsignedNoMajorLicenseFileName);

			int exitCode = XmlLicense.Sign(doc, new CryptoSettingsFromMachineStore(XmlLicenseTestSetupFixture.TestKeyContainterName));
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			Assert.Throws<MajorVersionNotFoundException>(() => new XmlLicenseService(XDocument.Load(new XmlNodeReader(doc)), XmlLicenseTestSetupFixture.PublicKeyXmlString, 234));
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
				if (_licenseService != null)
				{
					_licenseService.Dispose();
					_licenseService = null;
				}
			}
		}
	}
}