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

		public static string CurrentValidator
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator; }
			private set { StateHolderReader.Instance.StateReader.SessionScopeData.SikuliValidator = value; }
		}

		public static void SetTestMode(bool mode)
		{
			TestMode = mode;
		}

		public static void EnterValidator(IWin32Window owner)
		{
			if (!TestMode)
				return;
			using (var dialog = new SikuliEnterValidatorDialog())
			{
				dialog.ShowDialog(owner);
				if (dialog.DialogResult == DialogResult.OK)
				{
					CurrentValidator = dialog.GetValidatorName;
				}
			}
		}

		public static void ShowLoaded(IWin32Window owner)
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Loaded" };
			testView.ShowDialog(owner);
		}

		public static void ShowTaskDone(IWin32Window owner)
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView { Header = "Task Done" };
			testView.ShowDialog(owner);
		}

		public static void Validate(ISikuliValidator validator, IWin32Window owner)
		{
			if (!TestMode)
				return;
			var assertResult = validator.Validate();
			var testView = 
				new SikuliResultView { Header = "Task Done", Result = assertResult.Result, Details = assertResult.Details.ToString()};
			testView.ShowDialog(owner);
			CurrentValidator = SikuliValidatorRegister.None;
		}

	}
}
