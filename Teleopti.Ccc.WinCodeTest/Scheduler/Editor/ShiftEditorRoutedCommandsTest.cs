using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Editor;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Editor
{
    [TestFixture]
    public class ShiftEditorRoutedCommandsTest
    {
        private RoutedUICommand _command;

        [SetUp]
        public void Setup()
        {
            _command = ApplicationCommands.NotACommand;
        }

        [Test]
        public void VerifyEditMeetingCommand()
        {
            _command = ShiftEditorRoutedCommands.EditMeeting;
            Assert.AreEqual(_command.Text,UserTexts.Resources.EditMeeting);
        }

        [Test]
        public void VerifyCancelCommand()
        {
            _command = ShiftEditorRoutedCommands.Cancel;
            Assert.AreEqual(_command.Text, UserTexts.Resources.Cancel);
        }

        [Test]
        public void VerifyAddMainShiftCommand()
        {
            _command = ShiftEditorRoutedCommands.AddMainShift;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AddMainLayer);
        }

        [Test]
        public void VerifyAddActivityWithPeriodCommand()
        {
            _command = ShiftEditorRoutedCommands.AddActivityWithPeriod;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AddActivity);
        }
        [Test]
        public void VerifyAddOvertimeWithPeriodCommand()
        {
            _command = ShiftEditorRoutedCommands.AddOvertimeWithPeriod;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AddOvertime);
        }

        [Test]
        public void VerifyAddAbsenceWithPeriodCommand()
        {
            _command = ShiftEditorRoutedCommands.AddAbsenceWithPeriod;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AddAbsence);
        }

        [Test]
        public void VerifyAddPersonalWithPeriodShiftCommand()
        {
            _command = ShiftEditorRoutedCommands.AddPersonalShiftWithPeriod;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AddPersonalActivityThreeDots);
        }
    }
}
