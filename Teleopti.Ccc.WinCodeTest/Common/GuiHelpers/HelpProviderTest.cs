using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.WinCodeTest.Common.GuiHelpers
{
	[TestFixture]
	public class HelpProviderTest
	{
		private string _helpText;
		private CrossThreadTestRunner _testRunner;
		private FrameworkElement _target;

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
					  HelpProvider.SetHelpString(_target, _helpText);
					  Assert.AreEqual(_helpText, HelpProvider.GetHelpString(_target));
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
					 HelpProvider.SetHelpString(_target, _helpText);
					 Assert.IsTrue(ApplicationCommands.Help.CanExecute(null, _target));
				 });
		}

		//Dont know how to test this.....
		//[Test]
		//public void VerifyHelpIsCalledWhenCommandIsExecuted()
		//{

		//}
	}
}
