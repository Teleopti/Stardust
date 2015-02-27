﻿using System;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliHelper
	{
		public static class Popups
		{
			public static void ShowLoadedMessage(IWin32Window owner)
			{
				if (!InTestMode)
					return;
				var testView = new SikuliResultView { Header = "Loaded" };
				testView.ShowDialog(owner);
			}

			public static void ShowTaskDoneMessage(IWin32Window owner)
			{
				if (!InTestMode)
					return;
				var testView = new SikuliResultView { Header = "Task Done" };
				testView.ShowDialog(owner);
			}
		}

		private static TestDuration _timer;

		public static bool InTestMode
		{
			get { return StateHolderReader.Instance.StateReader.SessionScopeData.TestMode; }
			private set { StateHolderReader.Instance.StateReader.SessionScopeData.TestMode = value; }
		}

		public static void SetTestMode(bool mode)
		{
			InTestMode = mode;
		}

		public static class Validation
		{
			internal static string CurrentValidator { get; private set; }

			public static void EnterValidator(IWin32Window owner)
			{
				if (!InTestMode)
					return;
				using (var dialog = new SikuliEnterValidatorDialog())
				{
					dialog.ShowDialog(owner);
					if (dialog.DialogResult == DialogResult.OK)
					{
						CurrentValidator = dialog.GetValidatorName;
						_timer = new TestDuration();
						_timer.SetStart();
					}
				}
			}

			public static void Validate(ISikuliValidator validator, IWin32Window owner)
			{
				if (!InTestMode)
					return;
				if (_timer != null)
					_timer.SetEnd();
				var validationResult = validator.Validate(_timer);
				if (_timer != null)
				{
					validationResult.Details.AppendLine(String.Format("Duration = {0}", _timer.GetDuration().ToString(@"mm\:ss")));
					_timer = null;
				}
				validationResult.Details.AppendLine("Criterion: " + validator.Description);
				var testView = new SikuliResultView
				{
					Header = "Task Done",
					Result = validationResult.Result,
					Details = validationResult.Details.ToString()
				};
				testView.ShowDialog(owner);
				CurrentValidator = SikuliValidatorRegister.None;
			}
		}
	}
}
