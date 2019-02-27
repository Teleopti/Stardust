using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.Win.Scheduling.ScheduleReporting;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.ScheduleReporting
{
	public partial class ScheduleReportDialog : BaseDialogForm
	{

		private readonly ScheduleReportDialogSettings _settings;
		private const string settingName = "ScheduleReportDialog";
		private readonly bool _shiftsPerDay;

		public ScheduleReportDialog(bool shiftsPerDay)
		{
			_shiftsPerDay = shiftsPerDay;
			InitializeComponent();
			if (!DesignMode) SetTexts();
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_settings = PersonalSettingDataRepository.DONT_USE_CTOR(uow).FindValueByKey(settingName, new ScheduleReportDialogSettings());
				if(_shiftsPerDay && _settings.DetailLevel == ScheduleReportDetail.All) _settings.DetailLevel = ScheduleReportDetail.Break;
			}
		}

		public bool OneFile
		{
			get { return checkBoxSingleFile.Checked; }
		}

		public bool TeamReport
		{
			get
			{
				return radioButtonTeamReport.Checked;
			}
		}

		public bool Individual
		{
			get
			{
				return radioButtonIndividualReport.Checked;
			}
		}

		public ScheduleReportDetail DetailLevel
		{
			get
			{
				if (radioButtonNoDetails.Checked)
					return ScheduleReportDetail.None;

				if (radioButtonBreaksOnly.Checked)
					return ScheduleReportDetail.Break;

				return ScheduleReportDetail.All;
			}
		}

		public bool ShowPublicNote
		{
			get { return checkBoxShowPublicNote.Checked; }
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void scheduleReportDialogLoad(object sender, EventArgs e)
		{
			reportTypeSetting();
			singleFileSetting();
			detailLevelSetting();
			showPublicNoteSetting();

			if(_shiftsPerDay)
			{
				groupBox1.Enabled = false;
				radioButtonTeamReport.Checked = false;
				radioButtonIndividualReport.Checked = false;
				radioButtonAllDetails.Visible = false;
				radioButtonAllDetails.Checked = false;
			}
			else
			{
				checkBoxShowPublicNote.Visible = false;
			}

		}

		private void detailLevelSetting()
		{
			ScheduleReportDetail detailLevel = _settings.DetailLevel;
			switch (detailLevel)
			{
				case ScheduleReportDetail.None:
					radioButtonNoDetails.Checked = true;
					break;

				case ScheduleReportDetail.Break:
					radioButtonBreaksOnly.Checked = true;
					break;

				case ScheduleReportDetail.All:
					radioButtonAllDetails.Checked = true;
					break;
			}
		}

		private void singleFileSetting()
		{
			checkBoxSingleFile.Checked = _settings.SingleFile;
		}

		private void reportTypeSetting()
		{
			radioButtonIndividualReport.Checked = _settings.IndividualReport;
			radioButtonTeamReport.Checked = !_settings.IndividualReport;
		}

		private void showPublicNoteSetting()
		{
			checkBoxShowPublicNote.Checked = _settings.ShowPublicNote;
		}

		private void radioButtonIndividualReportCheckedChanged(object sender, EventArgs e)
		{
			checkBoxSingleFile.Enabled = radioButtonIndividualReport.Checked;
		}

		private void scheduleReportDialogFormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			if(DialogResult != System.Windows.Forms.DialogResult.OK)
				return;

			_settings.IndividualReport = radioButtonIndividualReport.Checked;
			_settings.SingleFile = checkBoxSingleFile.Checked;
			_settings.DetailLevel = DetailLevel;
			_settings.ShowPublicNote = checkBoxShowPublicNote.Checked;

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonalSettingDataRepository.DONT_USE_CTOR(uow).PersistSettingValue(_settings);
				uow.PersistAll();
			}
		}
	}
}
