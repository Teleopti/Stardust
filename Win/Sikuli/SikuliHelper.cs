using System.Windows.Forms;
using Teleopti.Ccc.Win.Sikuli.Helpers;
using Teleopti.Ccc.Win.Sikuli.Validators.RootValidators;
using Teleopti.Ccc.Win.Sikuli.Views;
using Teleopti.Interfaces.Domain;

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

		private static TestDuration _timer;

		public static bool InteractiveMode { get; private set; }

		public static void SetInteractiveMode(bool mode)
		{
			InteractiveMode = mode;
		}

		internal static string CurrentValidator { get; private set; }

		public static void EnterValidator(IWin32Window owner)
		{
			if (!InteractiveMode)
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

		public static void Validate(IRootValidator validator, IWin32Window owner)
		{
			if (!InteractiveMode)
				return;
			if (_timer != null)
				_timer.SetEnd();
			var validationResult = validator.Validate(_timer);
			validationResult.Details.AppendLine("Criteria:" + validator.Description);
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
