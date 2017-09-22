using NUnit.Framework;
using Teleopti.Ccc.WinCode.Reporting;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduledTimePerActivityTest
    {
        private ReportSettingsScheduledTimePerActivity _target;


        [SetUp]
        public void Setup()
        {
            _target = new ReportSettingsScheduledTimePerActivity();
        }

        [Test]
        public void ShouldCreateClass()
        {
            Assert.That(_target.Activities, Is.Not.Null);
        }
    }


}