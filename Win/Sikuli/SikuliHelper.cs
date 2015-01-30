using System;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliHelper
	{

		private class Duration
		{
			private DateTime _starTime;
			private DateTime _endTime;

			public void SetStart()
			{
				_starTime = DateTime.Now;
			}

			public void SetEnd()
			{
				_endTime = DateTime.Now;
			}

			public TimeSpan GetDuration()
			{
				return _endTime - _starTime;
			}
		}

		private static Duration _timer;

		public static bool TestMode
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TestMode; }
			private set { StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = value; }
		}

		public static string CurrentValidator { get; private set; }

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
					_timer = new Duration();
					_timer.SetStart();
				}
			}
		}

		public static void ShowLoaded(IWin32Window owner)
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView {Header = "Loaded"};
			testView.ShowDialog(owner);
		}

		public static void ShowTaskDone(IWin32Window owner)
		{
			if (!TestMode)
				return;
			var testView = new SikuliResultView {Header = "Task Done"};
			testView.ShowDialog(owner);
		}

		public static void Validate(ISikuliValidator validator, IWin32Window owner)
		{
			if (!TestMode)
				return;
			if (_timer != null)
				_timer.SetEnd();
			var validationResult = validator.Validate();
			if (_timer != null)
			{
				validationResult.Details.AppendLine(string.Format("Duration = {0}", _timer.GetDuration().ToString(@"mm\:ss")));
				_timer = null;
			}
			validationResult.Details.AppendLine("Criterion: " + validator.Description);
			var testView = new SikuliResultView { Header = "Task Done", Result = validationResult.Result, Details = validationResult.Details.ToString() };
			testView.ShowDialog(owner);
			CurrentValidator = SikuliValidatorRegister.None;
		}

	}
}
