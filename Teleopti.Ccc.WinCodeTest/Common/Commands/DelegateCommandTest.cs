using System;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
	[TestFixture]
	public class DelegateCommandTest
	{
		private ICommand _target;

		[SetUp]
		public void Setup()
		{
			_target = CreateCommand(ExecuteCommand);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void VerifyCanDelegateCommand()
		{
			_target.Execute(null);
			Assert.IsTrue(CommandHasBeenExecuted);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void CanExecute_WhenFuncReturnsFalse_ShouldReturnFalse()
		{
			_target = CreateCommand(ExecuteCommand, delegate { return false; });
			Assert.IsFalse(_target.CanExecute(null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void CanExecute_WhenFuncReturnsTrue_ShouldReturnTrue()
		{
			_target = CreateCommand(ExecuteCommand, delegate { return true; });
			Assert.IsTrue(_target.CanExecute(null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void CanExecute_WhenNoParameterForCanEexcuteIsSet_ShouldReturnTrue()
		{
			Assert.IsTrue(CreateCommand(ExecuteCommand).CanExecute(null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test, ExpectedException(typeof(ArgumentNullException))]
		public void CanExecuteFunc_WhenNull_ShouldThrowAException()
		{
			Assert.IsNotNull(CreateCommand(ExecuteCommand, null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test, ExpectedException(typeof(ArgumentNullException))]
		public void ExecuteAction_WhenNull_ShouldThrowException()
		{
			Assert.IsNotNull(new DelegateCommand(null));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void CanExecute_WhenCreatingADelegateCommandWithoutParameter_ShouldResponseToSuppliedFuncAsWithAnyOtherCommand()
		{
			var command = new DelegateCommand(ExecuteCommand, () => true);
			Assert.IsTrue(command.CanExecute());

			command = new DelegateCommand(ExecuteCommand, () => false);
			Assert.IsFalse(command.CanExecute());
		}

		#region syntax
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void VerifySyntax_ExecutingCommand_UsingGenericParameter()
		{
			string execParam = string.Empty;
			const string parameter = "parameter";

			_target = new DelegateCommand<string>(p => execParam = p);
			_target.Execute(parameter);
			Assert.AreEqual(parameter, execParam);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores"), Test]
		public void VerifySyntax_ExecuteCommand_WithoutParameter()
		{
			var command = new DelegateCommand(ExecuteCommand);
			command.Execute();
			Assert.IsTrue(CommandHasBeenExecuted);
		}
		#endregion

		#region helpers
		private static ICommand CreateCommand(Action<object> exec, Func<object, bool> canExec)
		{
			return new DelegateCommand<object>(exec, canExec);
		}

		private static ICommand CreateCommand(Action<object> exec)
		{
			return new DelegateCommand<object>(exec);
		}

		private void ExecuteCommand(object parameter)
		{
			CommandHasBeenExecuted = true;
		}

		private bool CommandHasBeenExecuted { get; set; }

		private void ExecuteCommand()
		{
			ExecuteCommand(null);
		}
		#endregion

	}
}