using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
    [TestFixture]
    public class CommonRoutedCommandsTest
    {

        private RoutedUICommand _command;
        private RoutedUICommand _command2;

        [SetUp]
        public void Setup()
        {
            _command = ApplicationCommands.NotACommand;
        }

        
        
        [Test]
        public void VerifyEditMeetingCommand()
        {
            _command = CommonRoutedCommands.EditMeeting;
            _command2 = CommonRoutedCommands.EditMeeting;
            Assert.AreEqual(_command.Text, UserTexts.Resources.EditMeeting);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyRemoveParticipantCommand()
        {
            _command = CommonRoutedCommands.RemoveParticipant;
            _command2 = CommonRoutedCommands.RemoveParticipant;
            Assert.AreEqual(_command.Text, UserTexts.Resources.RemoveParticipant);
            Assert.AreSame(_command,_command2);
        }

        [Test]
        public void VerifyDeleteMeetingCommand()
        {
            _command = CommonRoutedCommands.DeleteMeeting;
            _command2 = CommonRoutedCommands.DeleteMeeting;
            Assert.AreEqual(_command.Text, UserTexts.Resources.Delete);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyCreateMeetingCommand()
        {
            _command = CommonRoutedCommands.CreateMeeting;
            _command2 = CommonRoutedCommands.CreateMeeting;
            Assert.AreEqual(_command.Text, UserTexts.Resources.CreateMeeting);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMoveStartEarlierCommand()
        {
            _command = CommonRoutedCommands.MoveStartOneIntervalEarlier;
            _command2 = CommonRoutedCommands.MoveStartOneIntervalEarlier;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MoveStartOneIntervalEarlier);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMoveStartLaterCommand()
        {
            _command = CommonRoutedCommands.MoveStartOneIntervalLater;
            _command2 = CommonRoutedCommands.MoveStartOneIntervalLater;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MoveStartOneIntervalLater);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMoveEndLaterCommand()
        {
            _command = CommonRoutedCommands.MoveEndOneIntervalLater;
            _command2 = CommonRoutedCommands.MoveEndOneIntervalLater;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MoveEndOneIntervalLater);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMoveEndEarlierCommand()
        {
            _command = CommonRoutedCommands.MoveEndOneIntervalEarlier;
            _command2 = CommonRoutedCommands.MoveEndOneIntervalEarlier;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MoveEndOneIntervalEarlier);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMovePeriodEarlierCommand()
        {
            _command = CommonRoutedCommands.MovePeriodOneIntervalEarlier;
            _command2 = CommonRoutedCommands.MovePeriodOneIntervalEarlier;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MovePeriodOneIntervalEarlier);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyMovePeriodLaterCommand()
        {
            _command = CommonRoutedCommands.MovePeriodOneIntervalLater;
            _command2 = CommonRoutedCommands.MovePeriodOneIntervalLater;
            Assert.AreEqual(_command.Text, UserTexts.Resources.MovePeriodOneIntervalLater);
            Assert.AreSame(_command, _command2);
        }

        [Test]
        public void VerifyLoadPasswordPolicyCommand()
        {
            _command = CommonRoutedCommands.LoadPasswordPolicy;
            _command2 = CommonRoutedCommands.LoadPasswordPolicy;
            Assert.AreEqual(_command.Text, UserTexts.Resources.Load, "check usertext");
            Assert.AreSame(_command, _command2, "Check singelton");
        }

        [Test]
        public void VerifyToggleAutoUpdateCommand()
        {
            _command = CommonRoutedCommands.ToggleAutoUpdate;
            _command2 = CommonRoutedCommands.ToggleAutoUpdate;
            Assert.AreEqual(_command.Text, UserTexts.Resources.AutomaticUpdate, "check usertext");
            Assert.AreSame(_command, _command2, "Check singelton");
        }
    }
}

