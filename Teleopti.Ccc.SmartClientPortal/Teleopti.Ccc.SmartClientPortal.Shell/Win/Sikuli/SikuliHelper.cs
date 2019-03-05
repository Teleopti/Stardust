using System.Windows.Forms;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Helpers;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Validators.RootValidators;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Sikuli
{
	public static class SikuliHelper
	{

		//private static readonly IDictionary<string, IRootValidator> _activeValidators = new Dictionary<string, IRootValidator>();

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

		//internal static IDictionary<string, IRootValidator> ActiveValidators 
		//{
		//	get { return _activeValidators; }
		//}

		public static void EnterValidator(IWin32Window owner, ITimeZoneGuard timeZoneGuard)
		{
			if (!InteractiveMode)
				return;
			using (var dialog = new SikuliEnterValidatorDialog())
			{
				dialog.ShowDialog(owner);
				if (dialog.DialogResult == DialogResult.OK)
				{
					var validator = SikuliValidatorFactory.Scheduler.CreateValidator(dialog.GetValidatorName);
					CurrentValidator = validator;
					//ActiveValidators.Add(validator.Name, validator);
				}
			}
			if (CurrentValidator.InstantValidation)
				Validate(CurrentValidator, owner, timeZoneGuard);
		}

		public static void Validate(IRootValidator validator, IWin32Window owner, ITimeZoneGuard timeZoneGuard)
		{
			Validate(validator, owner, null, timeZoneGuard);
		}

		public static void Validate(IRootValidator validator, IWin32Window owner, object testData, ITimeZoneGuard timeZoneGuard)
		{
			if (!InteractiveMode)
				return;
			var validationResult = validator.Validate(testData, timeZoneGuard);
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
