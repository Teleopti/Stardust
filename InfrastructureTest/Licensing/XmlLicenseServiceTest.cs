using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class XmlLicenseServiceTest : IDisposable
    {
        private ILicenseService _licenseService;
        private MockRepository _mocks;
        private const string folder = "Licensing\\";
        private const string unsignedLicenseFileName = folder + "TeleoptiCCCUnsignedLicense.xml";
		private const string unsignedSeatLicenseFileName = folder + "TeleoptiCCCUnsignedSeatLicense.xml";
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        }

        [Test]
        [ExpectedException(typeof (SignatureValidationException))]
        public void VerifyConstructor()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            var reader = new StreamReader(licenseFileName);
            string licenseXmlString = reader.ReadToEnd();
            reader.Close();

            ILicense license = new License {XmlString = licenseXmlString};

        	var unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

            using (_mocks.Record())
            {
                Expect.Call(licenseRepository.LoadAll()).Return(new List<ILicense> {license}).Repeat.Once();
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            }
            using(_mocks.Playback())
            {
            	const int numberOfActiveAgents = 43289;

            	_licenseService = new XmlLicenseService(licenseRepository, numberOfActiveAgents);
            }
        }

        [Test, ExpectedException(typeof(LicenseMissingException))]
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
            using (_mocks.Playback())
            {
                _licenseService = new XmlLicenseService(unitOfWorkFactory, licenseRepository, personRepository);
            }
        }

        [Test, ExpectedException(typeof (LicenseMissingException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        public void VerifyExceptionInConstructor()
        {
            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            
            using(_mocks.Record())
            {
                Expect.Call(licenseRepository.LoadAll()).Return(new List<ILicense>());
            }
            using(_mocks.Playback())
            {
                _licenseService = new XmlLicenseService(licenseRepository, 4329);
            }
        }

        [Test]
        public void VerifyCreationOfValidLicense()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            _licenseService = new XmlLicenseService(licenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 10);

            Assert.AreEqual("This license is stolen!", _licenseService.CustomerName);
            Assert.AreEqual(new DateTime(2018, 02, 02, 12, 0, 0), _licenseService.ExpirationDate);
            Assert.AreEqual(new TimeSpan(30, 0, 0, 0), _licenseService.ExpirationGracePeriod);
            Assert.AreEqual(10000, _licenseService.MaxActiveAgents);
            Assert.AreEqual(new Percent(0.1), _licenseService.MaxActiveAgentsGrace);

            Assert.IsTrue(_licenseService.TeleoptiCccBaseEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccShiftTradesEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccHolidayPlannerEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPerformanceManagerEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccMyTimeWebEnabled);

            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersBaseEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersForecastsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersShiftsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersPeopleEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersAgentPortalEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersOptionsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersSchedulerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersIntradayEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersPermissionsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersReportsEnabled);

            Assert.IsFalse(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
        }

        /// <summary>
        /// test pilot license. This test assumes that signing key is in standard container
        /// </summary>
        [Test]
        public void VerifyCreationOfPilotCustomerLicense()
        {
            const string unsignedPilotCustomersLicenseFileName = folder + "TeleoptiCCCPilotCustomersUnsignedLicense.xml";
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedPilotCustomersLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName, licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            _licenseService = new XmlLicenseService(licenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 100);

            Assert.AreEqual("This pilot license is stolen!", _licenseService.CustomerName);
            Assert.GreaterOrEqual(new TimeSpan(31, 0, 0, 0), _licenseService.ExpirationGracePeriod);
            Assert.LessOrEqual(new TimeSpan(28, 0, 0, 0), _licenseService.ExpirationGracePeriod);
            Assert.AreEqual(1000, _licenseService.MaxActiveAgents);
            Assert.AreEqual(new Percent(1), _licenseService.MaxActiveAgentsGrace);

            Assert.IsFalse(_licenseService.TeleoptiCccBaseEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccShiftTradesEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccHolidayPlannerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPerformanceManagerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccMyTimeWebEnabled);

            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersBaseEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersForecastsEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersShiftsEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersPeopleEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersAgentPortalEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersOptionsEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersSchedulerEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersIntradayEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersPermissionsEnabled);
            Assert.IsTrue(_licenseService.TeleoptiCccPilotCustomersReportsEnabled);

            Assert.IsFalse(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
            File.Delete(licenseFileName);
        }

        /// <summary>
        /// test freemium license. This test assumes that signing key is in standard container
        /// </summary>
        [Test]
        public void VerifyCreationOfFreemiumLicense()
        {
            const string unsignedFreemiumLicenseFileName = folder + "TeleoptiCCCFreemiumUnsignedLicense.xml";
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedFreemiumLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName, licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            XDocument licenseXml = XDocument.Load(licenseFileName);
            _licenseService = new XmlLicenseService(licenseXml, XmlLicenseTestSetupFixture.PublicKeyXmlString, 1);

            Assert.AreEqual("This freemium license is stolen!", _licenseService.CustomerName);
            Assert.AreEqual(new TimeSpan(0, 20, 0, 0, 0), _licenseService.ExpirationGracePeriod);
            Assert.AreEqual(1, _licenseService.MaxActiveAgents);
            Assert.AreEqual(new Percent(0.2), _licenseService.MaxActiveAgentsGrace);

            Assert.IsFalse(_licenseService.TeleoptiCccBaseEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccAgentSelfServiceEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccShiftTradesEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccAgentScheduleMessengerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccHolidayPlannerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccRealTimeAdherenceEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPerformanceManagerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPayrollIntegrationEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccMyTimeWebEnabled);

            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersBaseEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersForecastsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersShiftsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersPeopleEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersAgentPortalEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersOptionsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersSchedulerEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersIntradayEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersPermissionsEnabled);
            Assert.IsFalse(_licenseService.TeleoptiCccPilotCustomersReportsEnabled);

            Assert.IsTrue(_licenseService.TeleoptiCccFreemiumForecastsEnabled);
            File.Delete(licenseFileName);
        }

        [Test, ExpectedException(typeof (LicenseMissingException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyLicenseFileMissingHandling()
        {
            string nonExistantFileName = Path.GetRandomFileName();

            new XmlLicenseService(nonExistantFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 432);
        }


        [Test, ExpectedException(typeof (LicenseExpiredException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyLicenseExpired()
        {
            const string unsignedExpiredLicenseFileName = folder + "TeleoptiCCCExpiredUnsignedLicense.xml";
            string expiredLicenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedExpiredLicenseFileName,
                                           XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           expiredLicenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            new XmlLicenseService(expiredLicenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 234);
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifySave()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            var personRepository = _mocks.StrictMock<IPersonRepository>();
            const int numberOfActiveAgents = 100;
               
            //ILicense oldLicense = new License {XmlString = "<foo></foo>"};

        	using(_mocks.Record())
            {
                Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
                licenseRepository.Add(new License());
                LastCall.IgnoreArguments();
            }
            using(_mocks.Playback())
            {
                ILicenseService licenseService =
                    XmlLicenseService.SaveNewLicense(licenseFileName, licenseRepository,
                                                     XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
                Assert.AreEqual(10000, licenseService.MaxActiveAgents);
            }
        }

        [Test]
        public void VerifySave2()
        {
            string licenseFileName = Path.GetTempFileName();
            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            var uow = _mocks.DynamicMock<IUnitOfWork>();
            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            var personRepository = _mocks.StrictMock<IPersonRepository>();
            
            const int numberOfActiveAgents = 100;

            using(_mocks.Record())
            {
                Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
                Expect.Call(uow.PersistAll()).Return(null);

                licenseRepository.Add(new License());
                LastCall.IgnoreArguments();
            }
            using(_mocks.Playback())
            {
                XmlLicenseService.SaveNewLicense(licenseFileName, uow, licenseRepository, XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
            }
        }

        [Test, ExpectedException(typeof(DataSourceException))]
        public void VerifySaveThrowsExceptionWhenHibernateError()
        {
            string licenseFileName = Path.GetTempFileName();
            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int)ExitCode.Success, exitCode);

            var uow = _mocks.DynamicMock<IUnitOfWork>(); 
            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            var personRepository = _mocks.StrictMock<IPersonRepository>();
            
            const int numberOfActiveAgents = 100;

            using (_mocks.Record())
            {
                Expect.Call(personRepository.NumberOfActiveAgents()).Return(numberOfActiveAgents);
                Expect.Call(uow.PersistAll()).Throw(new HibernateException());

                licenseRepository.Add(new License());
                LastCall.IgnoreArguments();
            }
            using(_mocks.Playback())
            {
                XmlLicenseService.SaveNewLicense(licenseFileName, uow, licenseRepository,
                                                 XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
            }
        }

        [Test, ExpectedException(typeof (TooManyActiveAgentsException))]
        public void VerifySaveFailsIfTooManyActiveAgents()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            var licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            var personRepository = _mocks.StrictMock<IPersonRepository>();
            Expect.Call(personRepository.NumberOfActiveAgents()).Return(20000);

           // ILicense oldLicense = new License {XmlString = "<foo></foo>"};

        	licenseRepository.Add(new License());
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            XmlLicenseService.SaveNewLicense(licenseFileName, licenseRepository,
                                             XmlLicenseTestSetupFixture.PublicKeyXmlString, personRepository);
        }

        [Test]
        public void VerifyActiveAgentsCheck()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            _licenseService = new XmlLicenseService(licenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 100);

            Assert.IsTrue(_licenseService.IsThisTooManyActiveAgents(11100));
            Assert.IsTrue(_licenseService.IsThisAlmostTooManyActiveAgents(10100));
        }

        [Test, ExpectedException(typeof (TooManyActiveAgentsException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyActiveAgentsException()
        {
            string licenseFileName = Path.GetTempFileName();

            int exitCode = XmlLicense.Sign(unsignedLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
                                           licenseFileName);
            Assert.AreEqual((int) ExitCode.Success, exitCode);

            new XmlLicenseService(licenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 11100);
        }

		[Test]
		public void ShouldRecalculateMaxAgentsIfSeatType()
		{
			string licenseFileName = Path.GetTempFileName();

			int exitCode = XmlLicense.Sign(unsignedSeatLicenseFileName, XmlLicenseTestSetupFixture.TestKeyContainterName,
										   licenseFileName);
			Assert.AreEqual((int)ExitCode.Success, exitCode);

			_licenseService = new XmlLicenseService(licenseFileName, XmlLicenseTestSetupFixture.PublicKeyXmlString, 1500);

			Assert.AreEqual("This license is stolen!", _licenseService.CustomerName);
			Assert.AreEqual(new DateTime(2018, 02, 02, 12, 0, 0), _licenseService.ExpirationDate);
			Assert.AreEqual(new TimeSpan(30, 0, 0, 0), _licenseService.ExpirationGracePeriod);
			Assert.AreEqual(new Percent(0.1), _licenseService.MaxActiveAgentsGrace);

			Assert.That(_licenseService.LicenseType, Is.EqualTo(LicenseType.Seat));
			Assert.That(_licenseService.MaxSeats, Is.EqualTo(1000));
			Assert.That(_licenseService.Ratio,Is.EqualTo(new decimal(2)));
			Assert.That(_licenseService.MaxActiveAgents,Is.EqualTo(2000));
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