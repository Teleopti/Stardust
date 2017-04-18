using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WinCodeTest.Common
{
    [TestFixture]
    public class PasteOptionsTest
    {
        PasteOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new PasteOptions();
        }

        [Test]
        public void VerifyPasteBehaviorProperty()
        {
            Assert.IsNotNull(_options.PasteBehavior);
            IPasteBehavior standardBehavior = new NormalPasteBehavior();
            _options.PasteBehavior = standardBehavior;
            Assert.AreEqual(standardBehavior, _options.PasteBehavior, "Set/Get");
        }

        [Test]
        public void CanCreatePasteOptions()
        {
            Assert.IsNotNull(_options);
        }

        [Test]
        public void CheckMainShiftGetSet()
        {
            _options.MainShift = true;
            Assert.AreEqual(true, _options.MainShift);
        }

		[Test]
		public void CheckMainShiftSpecialGetSet()
		{
			_options.MainShiftSpecial = true;
			Assert.AreEqual(true, _options.MainShiftSpecial);
		}

        [Test]
        public void CheckAbsenceGetSet()
        {
            Assert.AreEqual(PasteAction.Ignore, _options.Absences);
            _options.Absences = PasteAction.Add;
            Assert.AreEqual(PasteAction.Add, _options.Absences);
        }

        [Test]
        public void CheckDayOffGetSet()
        {
            _options.DayOff = true;
            Assert.AreEqual(true, _options.DayOff);
        }

        [Test]
        public void VerifyPersonalShifts()
        {
            Assert.AreEqual(false, _options.PersonalShifts);
            _options.PersonalShifts = true;
            Assert.AreEqual(true, _options.PersonalShifts);
        }

        [Test]
        public void VerifyDefault()
        {
            Assert.AreEqual(false, _options.Default);
            _options.Default = true;
            Assert.AreEqual(true, _options.Default);
        }

        [Test]
        public void VerifyDefaultDelete()
        {
            Assert.AreEqual(false, _options.DefaultDelete);
            _options.DefaultDelete = true;
            Assert.AreEqual(true, _options.DefaultDelete);
        }
        
        [Test]
        public void VerifyOvertime()
        {
            Assert.AreEqual(false,_options.Overtime);
            _options.Overtime = true;
            Assert.AreEqual(true, _options.Overtime);
        }

        [Test]
        public void VerifyPreference()
        {
            Assert.AreEqual(false, _options.Preference);
            _options.Preference = true;
            Assert.AreEqual(true, _options.Preference);
        }

        [Test]
        public void VerifyStudentAvailability()
        {
            Assert.AreEqual(false, _options.StudentAvailability);
            _options.StudentAvailability = true;
            Assert.AreEqual(true, _options.StudentAvailability);
        }

        [Test]
        public void VerifyOvertimeAvailability()
        {
            Assert.AreEqual(false, _options.OvertimeAvailability);
            _options.OvertimeAvailability  = true;
            Assert.AreEqual(true, _options.OvertimeAvailability );
        }

		[Test]
		public void VerifyShiftAsOvertime()
		{
			Assert.AreEqual(false, _options.ShiftAsOvertime);
			_options.ShiftAsOvertime = true;
			Assert.AreEqual(true, _options.ShiftAsOvertime);
		}
    }
}
