using System;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win
{
	public static class SikuliHelper
	{
		private static SikuliValidatorRegister.Select _currentValidator = SikuliValidatorRegister.Select.None;

		public static bool TestMode
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TestMode; }
			set { StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = value; }
		}

		public static string SikuliValidator
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator; }
			set { StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator = value; }
		}

		public static void RegisterValidator(SikuliValidatorRegister.Select validator)
		{
			if (!TestMode)
				return;
			_currentValidator = validator;
		}

		public static SikuliValidatorRegister.Select CurrentValidator
		{
			get
			{
				return _currentValidator;
			}
		}

		public static void ShowLoaded()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Loaded" };
			testView.ShowDialog();
			_currentValidator = SikuliValidatorRegister.Select.None;
		}

		public static void ShowTaskDone()
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Task Done" };
			testView.ShowDialog();
			_currentValidator = SikuliValidatorRegister.Select.None;
		}

		public static void AssertValidation(Func<SikuliValidationResult> assertFunc)
		{
			if (!TestMode)
				return;
			var assertResult = assertFunc();
			var testView = 
				new SikuliResultView { Header = "Task Done", Result = assertResult.Result, Details = assertResult.Details.ToString()};
			testView.ShowDialog();
			_currentValidator = SikuliValidatorRegister.Select.None;
		}

		public static void InputValidator()
		{
			using (var dialog = new SikuliInputBox())
			{
				if (dialog.DialogResult == DialogResult.OK)
				{
					SikuliValidator = dialog.GetValidatorName;
				}
			}
		}

	}
}
