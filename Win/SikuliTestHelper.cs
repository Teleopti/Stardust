using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win
{
	public class SikuliTestHelper
	{
		private SikuliTestRegister.Select _currentTest = SikuliTestRegister.Select.None;

		public static void StartTestMode()
		{
			StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = true;
		}

		public static bool TestMode
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TestMode; }
		}

		public void RegisterTest(SikuliTestRegister.Select test)
		{
			if (!TestMode)
				return;
			_currentTest = test;
		}

		public SikuliTestRegister.Select CurrentTest
		{
			get
			{
				return _currentTest;
			}
		}

		public void AssertLoaded()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Loaded" };
			testView.ShowDialog();
			_currentTest = SikuliTestRegister.Select.None;
		}

		public void AssertTestDone()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Task Done" };
			testView.ShowDialog();
			_currentTest = SikuliTestRegister.Select.None;
		}

		public void AssertTest(Func<bool> assertFunc)
		{
			if (!TestMode)
				return;
			bool assertResult = assertFunc();
			var testView = new SikuliResultView { Header = "Task Done", Result = assertResult };
			testView.ShowDialog();
			_currentTest = SikuliTestRegister.Select.None;
		}

	}
}
