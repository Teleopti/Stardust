using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
    [TestFixture]
    public class PersonCultureSetterTest
    {
        private SetThreadCulture _target;
        private IPerson _person;
        private MockRepository _mocks;
        private IPermissionInformation _permissionInformation;
        private CultureInfo _cultureBeforeTest;
        private CultureInfo _uiCultureBeforeTest;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = _mocks.StrictMock<IPerson>();
            _permissionInformation = _mocks.StrictMock<IPermissionInformation>();
            _target = new SetThreadCulture();
            _cultureBeforeTest = CultureInfo.CurrentCulture;
            _uiCultureBeforeTest = CultureInfo.CurrentUICulture;
        }

        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = _cultureBeforeTest;
            Thread.CurrentThread.CurrentUICulture = _uiCultureBeforeTest;
        }

        [Test]
        public void ShouldSetCultureToArabic()
        {
            CultureInfo arabicCulture = CultureInfo.GetCultureInfo("ar-SA");

                _target.SetCulture(new Regional(CccTimeZoneInfoFactory.StockholmTimeZoneInfo(),arabicCulture,arabicCulture));
        
            Assert.AreEqual(Thread.CurrentThread.CurrentCulture, arabicCulture);
            Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, arabicCulture);
        }

        [Test,Ignore("Robin will fix this test!")]
        public void ShouldNotSetCultureWhenPersonHasComputerDefaultSettings()
        {
            using (_mocks.Record())
            {
                Expect.Call(_person.PermissionInformation).Return(_permissionInformation).Repeat.AtLeastOnce();
                Expect.Call(_permissionInformation.CultureLCID()).Return(null);
                Expect.Call(_permissionInformation.UICultureLCID()).Return(null);
            }

            using (_mocks.Playback())
            {
                _target.SetCulture(null);
            }

            Assert.AreEqual(_cultureBeforeTest, Thread.CurrentThread.CurrentCulture);
            Assert.AreEqual(_uiCultureBeforeTest, Thread.CurrentThread.CurrentUICulture);
        }
    }
}
