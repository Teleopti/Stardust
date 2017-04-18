using System;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class PersonFinderSettingsTest
    {
        private PersonFinderSettings _target;

        [SetUp]
        public void Setup()
        {
            _target = new PersonFinderSettings();
        }

        [Test]
        public void ShouldSetDefaultValuesInConstructor()
        {
            Assert.AreEqual(RowsPerPage.Ten, _target.NumberOfRowsPerPage);
            Assert.AreEqual(DateTime.Today.AddMonths(-1), _target.TerminalDate);
        }
    }
}
