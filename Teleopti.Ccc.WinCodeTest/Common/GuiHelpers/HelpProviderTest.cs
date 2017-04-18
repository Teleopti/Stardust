using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Interop;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
	[TestFixture]
	public class HelpProviderTest
	{
		private string _helpText;
		private CrossThreadTestRunner _testRunner;
		private IInputElement _target;

		[SetUp]
		public void Setup()
		{
			_testRunner = new CrossThreadTestRunner();
		}
	
		[Test]
		public void VerifyCanSetHelpCommandOnFrameworkElement()
		{
			_testRunner.RunInSTA(
				  delegate
				  {
					  _helpText = "help";
					  _target = new Button();
					  HelpProvider.SetHelpString((DependencyObject) _target, _helpText);
					  Assert.AreEqual(_helpText, HelpProvider.GetHelpString((DependencyObject) _target));
				  });
		 
		}

		[Test]
		public void VerifyHelpCanExecuteIfHelpIsSetOnElement()
		{
			_testRunner.RunInSTA(
				 delegate
				 {
					 _helpText = "help";
					 _target = new Button();
					 Assert.IsFalse(ApplicationCommands.Help.CanExecute(null, _target));
					 HelpProvider.SetHelpString((DependencyObject) _target, _helpText);
					 Assert.IsTrue(ApplicationCommands.Help.CanExecute(null, _target));
				 });
		}

		[Test]
		public void VerifyHelpCanExecuteIfHelpIsSetOnAdornerDecorator()
		{
			_testRunner.RunInSTA(
				 delegate
				 {
					 _helpText = "help";
					 var button = new Button();
					 _target = new AdornerDecorator {Child = button};
					 Assert.IsFalse(ApplicationCommands.Help.CanExecute(null, _target));
					 HelpProvider.SetHelpString(button, _helpText);
					 Assert.IsTrue(ApplicationCommands.Help.CanExecute(null, _target));
				 });
		}

		[Test]
		public void VerifyHelpCanExecuteIfHelpIsSetOnPanel()
		{
			_testRunner.RunInSTA(
				 delegate
				 {
					 _helpText = "help";
					 var button = new Button();
					 var dockPanel = new DockPanel();
					 dockPanel.Children.Add(button);
					 _target = dockPanel;
					 Assert.IsFalse(ApplicationCommands.Help.CanExecute(null, _target));
					 HelpProvider.SetHelpString(button, _helpText);
					 Assert.IsTrue(ApplicationCommands.Help.CanExecute(null, _target));
				 });
		}

		[Test]
		public void VerifyHelpCanExecuteIfHelpIsSetOnMultipleViewHost()
		{
			_testRunner.RunInSTA(
				 delegate
				 {
					 _helpText = "help";
					 var button = new Button();
					 var model = new MultipleHostViewModel();
					 model.Add("test",button);

					 _target = new MyOddFakeWPFControl { Model = model };
					 Assert.IsFalse(ApplicationCommands.Help.CanExecute(null, _target));
					 HelpProvider.SetHelpString(button, _helpText);
					 Assert.IsTrue(ApplicationCommands.Help.CanExecute(null, _target));
				 });
		}

		private class MyOddFakeWPFControl : Control, IMultipleHostControl
		{
			public IMultipleHostViewModel Model { get; set; }
		}
	}
}
