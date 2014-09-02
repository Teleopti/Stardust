using System;
using System.IO;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Forecasting.Forms.ExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
	public partial class ImportForecastView : BaseDialogForm, IImportForecastView
	{
		public ImportForecastView()
		{
			InitializeComponent();
			if (!DesignMode)
				SetTexts();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			Presenter.Initialize();
		}

		internal ImportForecastPresenter Presenter { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
		public void SetWorkloadName(string name)
		{
			var workloadToolTip = new ToolTip { ShowAlways = true };
			workloadToolTip.SetToolTip(labelWorkloadName, name);

			if (name.Length >= 40)
				name = name.Substring(0, 37) + "...";

			labelWorkloadName.Text = name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetSkillName(string name)
		{
			var skillToolTip = new ToolTip { ShowAlways = true };
			skillToolTip.SetToolTip(txtSkillName, name);

			if (name.Length >= 40)
				name = name.Substring(0, 37) + "...";

			txtSkillName.Text = name;
			skillToolTip.Dispose();
		}

		private void browseImportFileButtonClick(object sender, EventArgs e)
		{
			var result = openFileDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				textBoxImportFileName.Text = openFileDialog.FileName;
			}
		}

		void radioButtonImportWlAndStaffingCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonImportWLAndStaffing.Checked)
				Presenter.SetImportType(ImportForecastsMode.ImportWorkloadAndStaffing);
		}

		void radioButtonImportWorkloadCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonImportWorkload.Checked)
				Presenter.SetImportType(ImportForecastsMode.ImportWorkload);
		}

		void radioButtonImportStaffingCheckedChanged(object sender, EventArgs e)
		{
			if (radioButtonImportStaffing.Checked)
				Presenter.SetImportType(ImportForecastsMode.ImportStaffing);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.MessageBoxAdv.Show(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Forecasting.Forms.ExportPages.JobStatusView.SetMessage(System.String)")]
		private void buttonAdvImportClick(object sender, EventArgs e)
		{
			Presenter.StartImport(textBoxImportFileName.Text);
		}

		public void ShowValidationException(string message)
		{
			ShowErrorMessage(message, UserTexts.Resources.ValidationError);
		}

		public void ShowError(string errorMessage)
		{
			ShowErrorMessage(errorMessage, UserTexts.Resources.ImportError);
		}

		public void ShowStatusDialog(Guid jobId)
		{
			var statusDialog = new JobStatusView(new JobStatusModel { JobStatusId = jobId, ProgressMax = 100 });
			statusDialog.ShowDialog();
		}

		public void SetVisibility(ISkillType skillType)
		{
			if (skillType.ForecastSource != ForecastSource.InboundTelephony)
			{
				radioButtonImportStaffing.Visible = false;
				radioButtonImportWLAndStaffing.Visible = false;
			}
		}

		private void textBoxImportFileNameTextChanged(object sender, EventArgs e)
		{
			buttonAdvImport.Enabled = File.Exists(textBoxImportFileName.Text);
		}

	}
}
