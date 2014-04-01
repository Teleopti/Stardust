using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    public class LicenseVerifierTest
    {
        private MockRepository _mocks;
        private ILicenseFeedback _licenseFeedback;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ILicenseRepository _licenseRepository;
        private ILicenseService _licenseService;
        private LicenseVerifier _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _licenseFeedback = _mocks.StrictMock<ILicenseFeedback>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _licenseRepository = _mocks.StrictMock<ILicenseRepository>();
            _licenseService = _mocks.StrictMock<ILicenseService>();
        }

        [Test]
        public void ShouldCheckDate()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_licenseService.ExpirationDate).Return(DateTime.Today.AddDays(2)).Repeat.Twice();
                Expect.Call(_licenseService.ExpirationGracePeriod).Return(TimeSpan.FromDays(3));
                Expect.Call(() => _licenseFeedback.Warning(string.Empty, string.Empty)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => _licenseService, _licenseFeedback, _unitOfWorkFactory,
                                                     _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Not.Be.Null();
            }
        }

        [Test]
        public void ShouldCheckSignatureException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
				Expect.Call(_unitOfWorkFactory.Name).Return("datasource name");
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw new SignatureValidationException (); }, _licenseFeedback,
                                                    _unitOfWorkFactory,  _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

        [Test]
        public void ShouldCheckLicenseMissingException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
				Expect.Call(_unitOfWorkFactory.Name).Return("datasource name");
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw new LicenseMissingException (); }, _licenseFeedback,
                                                    _unitOfWorkFactory, _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

        [Test]
        public void ShouldCheckLicenseExpiredException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
            	Expect.Call(_unitOfWorkFactory.Name).Return("datasource name");
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw new LicenseExpiredException(); }, _licenseFeedback,
                                                    _unitOfWorkFactory, _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

        [Test]
        public void ShouldCheckXmlException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw new XmlException(); }, _licenseFeedback,
                                                    _unitOfWorkFactory, _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

        [Test]
        public void ShouldCheckFormatException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw new FormatException(); }, _licenseFeedback,
                                                    _unitOfWorkFactory, _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

        [Test]
        public void ShouldCheckTooManyActiveAgentsException()
        {
            var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(() => _licenseFeedback.Error(string.Empty)).IgnoreArguments();
				Expect.Call(_unitOfWorkFactory.Name).Return("datasource name");
            }

            TooManyActiveAgentsException ex = new TooManyActiveAgentsException(1, 2);
            using (_mocks.Playback())
            {
                _target = new licenseVerifierForTest(() => { throw ex; }, _licenseFeedback,
                                                    _unitOfWorkFactory, _licenseRepository);
                _target.LoadAndVerifyLicense().Should().Be.Null();
            }
        }

		[Test]
		public void ShouldReturnMessageBasedOnAgentsOrSeats()
		{
			var unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
			var message = "datasource name" + "\r\n" + string.Format(CultureInfo.CurrentCulture, Resources.YouHaveTooManySeats, 1);
			using (_mocks.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(() => _licenseFeedback.Error(message));
				Expect.Call(_unitOfWorkFactory.Name).Return("datasource name");
			}
			using (_mocks.Playback())
			{
				_target = new licenseVerifierForTest(() => { throw new TooManyActiveAgentsException(1, 2,LicenseType.Seat); }, _licenseFeedback,
													_unitOfWorkFactory, _licenseRepository);
				_target.LoadAndVerifyLicense().Should().Be.Null();
			}
		}

        private class licenseVerifierForTest : LicenseVerifier
        {
            private readonly Func<ILicenseService> _licenseServiceFunction;

            public licenseVerifierForTest(Func<ILicenseService> licenseServiceFunction, ILicenseFeedback licenseFeedback, IUnitOfWorkFactory unitOfWorkFactory, ILicenseRepository licenseRepository) : base(licenseFeedback, unitOfWorkFactory, licenseRepository)
            {
                _licenseServiceFunction = licenseServiceFunction;
            }

            protected override ILicenseService XmlLicenseService()
            {
                return _licenseServiceFunction();
            }
        }
    }
}
