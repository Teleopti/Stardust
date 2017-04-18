using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
	[TestFixture]
	public class RoutedEventBehaviorFactoryTest
	{
		private MockRepository _mocker;
		private CrossThreadTestRunner _testRunner;
		private DependencyProperty _property;

		[SetUp]
		public void Setup()
		{
			_mocker = new MockRepository();
			_testRunner = new CrossThreadTestRunner();
		}

		[Test]
		public void VerifyFactoryShouldReturnDependencyProperty()
		{
			_property = EventBehaviorFactory.CreateCommandExecutionEventBehavior(FrameworkElement.LoadedEvent, "LoadedCommand", typeof(RoutedEventBehaviorFactoryTest));
			Assert.IsNotNull(_property);
		}

		[Test]
		public void VerifyCommandShouldBeExecutedWhenEventIsRaised()
		{
			RoutedEventArgs e = new RoutedEventArgs(TextBox.TextChangedEvent);
			ICommand command = _mocker.StrictMock<ICommand>();
			using (_mocker.Record())
			{
				Expect.Call(command.CanExecute(e)).Return(true);
				command.Execute(e);
			}
			_testRunner.RunInSTA(
				delegate
					{
						using (_mocker.Playback())
						{
							TextBox textBox = new TextBox();
							TextBoxBehavior.SetTextChangedCommand(textBox, command);
							textBox.RaiseEvent(e);
						}
					});
		}

		[Test]
		public void VerifyCommandShouldNotBeExecutedWhenCanExecuteReturnsFalse()
		{
			RoutedEventArgs e = new RoutedEventArgs(TextBox.TextChangedEvent);
			ICommand command = _mocker.StrictMock<ICommand>();
			using (_mocker.Record())
			{
				Expect.Call(command.CanExecute(null)).Return(false).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(() => command.Execute(null)).IgnoreArguments().Repeat.Never();
				//Make sure Execute doesnt get called
			}

			_testRunner.RunInSTA(
				delegate
					{
						using (_mocker.Playback())
						{
							TextBox textBox = new TextBox();
							TextBoxBehavior.SetTextChangedCommand(textBox, command);
							textBox.RaiseEvent(e);
						}
					});
		}

		[Test]
		public void VerifyCommandShouldNotBeExecuted()
		{
			RoutedEventArgs e = new RoutedEventArgs(TextBox.TextChangedEvent);
			ICommand command = _mocker.StrictMock<ICommand>();
			using (_mocker.Record())
			{
				//Make sure Execute doesnt get called
				Expect.Call(command.CanExecute(null)).IgnoreArguments().Repeat.Never();
				//Make sure CanExecute doesnt get called
				Expect.Call(() => command.Execute(null)).IgnoreArguments().Repeat.Never();

			}
			_testRunner.RunInSTA(
				delegate
					{
						using (_mocker.Playback())
						{
							TextBox textBox = new TextBox();
							TextBoxBehavior.SetTextChangedCommand(textBox, command);
							TextBoxBehavior.SetTextChangedCommand(textBox, null);
							textBox.RaiseEvent(e); //Should not be executed when removed
						}
					});
		}
	}
}