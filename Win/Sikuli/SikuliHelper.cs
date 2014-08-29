using System;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliHelper
	{

		public static bool TestMode
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TestMode; }
			private set { StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = value; }
		}

		public static string SikuliValidator
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator; }
			private set { StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator = value; }
		}

		public static void SetTestMode(bool mode)
		{
			TestMode = mode;
		}

		public static void EnterValidator()
		{
			using (var dialog = new SikuliEnterValidatorDialog())
			{
				dialog.ShowDialog();
				if (dialog.DialogResult == DialogResult.OK)
				{
					SikuliValidator = dialog.GetValidatorName;
				}
			}
		}

		public static void ShowLoaded()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Loaded" };
			testView.ShowDialog();
		}

		public static void ShowTaskDone()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Task Done" };
			testView.ShowDialog();
		}

		public static void AssertValidation(Func<SikuliValidationResult> assertFunc)
		{
			if (!TestMode)
				return;
			var assertResult = assertFunc();
			var testView = 
				new SikuliResultView { Header = "Task Done", Result = assertResult.Result, Details = assertResult.Details.ToString()};
			testView.ShowDialog();
			SikuliValidator = SikuliValidatorRegister.SelectValidator.None;
		}

	}
}
