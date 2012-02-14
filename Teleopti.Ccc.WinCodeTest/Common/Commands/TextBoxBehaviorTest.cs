﻿using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Commands
{
	

	[TestFixture]
	public class TextBoxBehaviorTest
	{
	 
		private CrossThreadTestRunner _testRunner;
   
		[SetUp]
		public void Setup()
		{
			_testRunner = new CrossThreadTestRunner();
		}
	
	
		[Test]
		public void VerifyTextChangedProperty()
		{
			_testRunner.RunInSTA(
			  delegate
			  {
				  TextBox textBox = new TextBox();
				  TextBoxBehavior.SetTextChangedCommand(textBox, ApplicationCommands.NotACommand);
				  Assert.AreEqual(ApplicationCommands.NotACommand, TextBoxBehavior.GetTextChangedCommand(textBox));
			  });
		}
	}
}
