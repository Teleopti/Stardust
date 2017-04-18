using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands.CommandBehaviors;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands.CommandBehaviors
{

    /// <summary>
    /// These tests wraps the whole CommandBehavior system, consisting of:
    /// BehaviorBinding.cs
    /// CommandBehavior.cs
    /// CommandBehaviorBinding.cs
    /// CommandBehaviorCollection.cs
    /// EventHandlerGenerator.cs
    /// ExecutionStrategy.cs
    /// </summary>
    /// <remarks>
    /// Set as longrunning because it takes some time to create the visuals.....
    /// </remarks>
    //[TestFixture, Category("Longrunnig")]
    [TestFixture]
    public class CommandBehaviorTest
    {
        private MockRepository _mocker;
        private string _parameter;
        private object _actionResult;


        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _parameter = "parameter";
        }

        [Test]
        public void VerifyProperties()
        {
            ICommand command = _mocker.StrictMock<ICommand>();
            Action<object> action = delegate{ };
            DependencyObject dep = new DependencyObject();

            CommandBehavior.SetAction(dep,action);
            CommandBehavior.SetCommandParameter(dep,_parameter);
            CommandBehavior.SetCommand(dep,command);

            Assert.AreEqual(CommandBehavior.GetAction(dep),action);
            Assert.AreEqual(CommandBehavior.GetCommand(dep),command);
            Assert.AreEqual(CommandBehavior.GetCommandParameter(dep),_parameter);
        }

       

        [Test, Apartment(ApartmentState.STA)]
        public void VerifyCommandShouldBeExecutedWhenEventIsRaised()
        {

            ICommand command = _mocker.StrictMock<ICommand>();
            TextBox textBox = _mocker.PartialMock<TextBox>();
            using (_mocker.Record())
            {
                Expect.Call(command.CanExecute(_parameter)).Return(true);
                Expect.Call(command.CanExecute(_parameter)).Return(false);//Return false the second time
                command.Execute(_parameter);//Make sure its only called once
            }

            using (_mocker.Playback())
            {
                CommandBehavior.SetEvent(textBox, "TextChanged");
                CommandBehavior.SetCommandParameter(textBox, _parameter);
                CommandBehavior.SetCommand(textBox, command);
                textBox.RaiseEvent(new TextChangedEventArgs(TextBoxBase.TextChangedEvent, new UndoAction()));
                textBox.RaiseEvent(new TextChangedEventArgs(TextBoxBase.TextChangedEvent, new UndoAction()));//Raising one more time should not invoke the command since CanExecute=false
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifyCommandRebindsToNewEvent()
        {
            ICommand command = _mocker.StrictMock<ICommand>();
            TextBox textBox = _mocker.PartialMock<TextBox>();
            using (_mocker.Record())
            {
                Expect.Call(command.CanExecute(_parameter)).Return(true);
                command.Execute(_parameter);
            }

            using (_mocker.Playback())
            {
                CommandBehavior.SetEvent(textBox, "SizeChanged");//Will not be called
                CommandBehavior.SetEvent(textBox, "TextChanged");
                CommandBehavior.SetCommandParameter(textBox, _parameter);
                CommandBehavior.SetCommand(textBox, command);
                textBox.RaiseEvent(new TextChangedEventArgs(TextBoxBase.TextChangedEvent, new UndoAction()));
            }
        }

		[Test, Apartment(ApartmentState.STA)]
		public void VerifyCommandBindingToAction()
        {
            CommandBehaviorBinding target = new CommandBehaviorBinding();

            target.Action = new Action<object>(anotherAction);
            target.Action = new Action<object>(action);
            target.CommandParameter = _parameter;
            target.Execute();
            Assert.AreEqual(_parameter,_actionResult);
        }

       

        [Test]
        public void VerifyThatCommandExecutingStrategyMustHaveBehavior()
        {
            CommandExecutionStrategy strategy = new CommandExecutionStrategy();
            Assert.Throws<InvalidOperationException>(() => strategy.Execute(_parameter));
        }

        [Test]
        public void VerifyCannotBindToEventThatDosNotExistOnTheTargetElement()
        {
            Assert.Throws<InvalidOperationException>(() => CommandBehavior.SetEvent(new DependencyObject(), _parameter)); //Parameter does not exist as event 
        }

        #region actions
        private void action(object obj)
        {
            _actionResult = obj;
        }

        private void anotherAction(object obj)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}