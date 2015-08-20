﻿using System.Windows.Forms;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;
using Teleopti.Ccc.Win.Sikuli.Views;

namespace Teleopti.Ccc.Win.Sikuli
{
	public static class SikuliHelper
	{
		public static void ShowLoadedView(IWin32Window owner)
		{
			if (!InteractiveMode)
				return;
			var testView = new SikuliResultView {Header = "Loaded"};
			testView.ShowDialog(owner);
		}

		public static void ShowTaskDoneView(IWin32Window owner)
		{
			if (!InteractiveMode)
				return;
			var testView = new SikuliResultView {Header = "Task Done"};
			testView.ShowDialog(owner);
		}

		public static bool InteractiveMode { get; private set; }

		public static void SetInteractiveMode(bool mode)
		{
			InteractiveMode = mode;
		}

		internal static IRootValidator CurrentValidator { get; private set; }

		public static void EnterValidator(IWin32Window owner)
		{
			if (!InteractiveMode)
				return;
			using (var dialog = new SikuliEnterValidatorDialog())
			{
				dialog.ShowDialog(owner);
				if (dialog.DialogResult == DialogResult.OK)
				{
					CurrentValidator = SikuliValidatorFactory.Scheduler.CreateValidator(dialog.GetValidatorName);
				}
			}
			if (!CurrentValidator.ExplicitValidation)
				Validate(CurrentValidator, owner);
		}

		public static void Validate(IRootValidator validator, IWin32Window owner)
		{
			Validate(validator, owner, null);
		}

		public static void Validate(IRootValidator validator, IWin32Window owner, object testData)
		{
			if (!InteractiveMode)
				return;
			var validationResult = validator.Validate(testData);
			handleValidationResult(validator, owner, validationResult);
		}

		private static void handleValidationResult(IRootValidator validator, IWin32Window owner, SikuliValidationResult validationResult)
		{
			validationResult.Details.AppendLine("Criteria: " + validator.Description);
			var testView = new SikuliResultView
			{
				Header = "Task Done",
				Result = validationResult.Result,
				Details = validationResult.Details.ToString()
			};
			testView.ShowDialog(owner);
			CurrentValidator = null;
		}
	}
}
