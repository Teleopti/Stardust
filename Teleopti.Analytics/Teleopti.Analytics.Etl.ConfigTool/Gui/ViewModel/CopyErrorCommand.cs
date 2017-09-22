using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class CopyErrorCommand : ICommand
	{
		private readonly JobHistoryTreeViewModel _model;
		private bool _canExecute;

		public CopyErrorCommand(JobHistoryTreeViewModel model)
		{
			_model = model;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		public void Execute(object parameter)
		{
			if (_model == null || _model.SelectedItem == null)
				return;

			var msg = new StringBuilder();
			msg.AppendLine("EXCEPTION MESSAGE");
			msg.AppendLine("===========================");
			msg.AppendLine(_model.SelectedItem.ErrorMessage);
			msg.AppendLine("");
			msg.AppendLine("EXCEPTION STACKTRACE");
			msg.AppendLine("===========================");
			msg.AppendLine(_model.SelectedItem.ErrorStackTrace);
			msg.AppendLine("");
			msg.AppendLine("INNER EXCEPTION MESSAGE");
			msg.AppendLine("===========================");
			msg.AppendLine(_model.SelectedItem.InnerErrorMessage);
			msg.AppendLine("");
			msg.AppendLine("INNER EXCEPTION STACKTRACE");
			msg.AppendLine("===========================");
			msg.AppendLine(_model.SelectedItem.InnerErrorStackTrace);

			//sometimes it fails for some (external) reason
			try
			{
				Clipboard.SetDataObject(msg.ToString(), true, 10, 10);
			}
			catch (ExternalException)
			{
				const string errorMessage = "The Clipboard is being used by another program, please try again later.";
				MessageBox.Show(errorMessage, "Copy Error Failure");
			}
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute;
		}

		public void SetCanExecute()
		{
			_canExecute = _model.SelectedItem != null && !_model.SelectedItem.Success;
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, new EventArgs());
		}

		public event EventHandler CanExecuteChanged;
	}
}
