using System;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands.CommandBehaviors;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands.CommandBehaviors
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [TestFixture]
    public class CommandBehaviorBindingTest
    {
        private CommandBehaviorBinding _target;

        [SetUp]
        public void Setup()
        {
            _target = new CommandBehaviorBinding();
        }

        [Test]
        public void VerifyProperties()
        {
            _target.Command = ApplicationCommands.Paste;
            _target.Command = ApplicationCommands.SelectAll;
            _target.Action = new Action<object>(action);
            _target.Action = new Action<object>(anotherAction);
            Assert.AreEqual(_target.Command,ApplicationCommands.SelectAll);
            Assert.AreEqual(_target.Action,new Action<object>(anotherAction));
        }

        private void anotherAction(object obj)
        {
            throw new NotImplementedException();
        }

        private void action(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
