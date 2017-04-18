using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Reporting;

namespace Teleopti.Ccc.WinCodeTest.Presentation
{
    [TestFixture]
    public class ReportSettingsScheduleAuditingTest
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldCreateClass()
        {
            var target = new ReportSettingsScheduleAuditing();
            Assert.That(target.Agents.Count, Is.EqualTo(0));
        }
    }
}