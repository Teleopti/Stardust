using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    public partial class ScheduleReportDialog : BaseRibbonForm
    {

        private readonly ScheduleReportDialogSettings _settings;
        private const string SettingName = "ScheduleReportDialog";

        public ScheduleReportDialog()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                _settings = new PersonalSettingDataRepository(uow).FindValueByKey(SettingName, new ScheduleReportDialogSettings());
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

        public bool ShiftPerDay
        {
            get
            {
                return radioButtonShiftsPerDay.Checked;
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

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void setColor()
        {
            BackColor = ColorHelper.DialogBackColor();
        }

        private void ScheduleReportDialog_Load(object sender, EventArgs e)
        {
            setColor();

            reportTypeSetting();
            singleFileSetting();
            detailLevelSetting();
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

        private void radioButtonIndividualReport_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxSingleFile.Enabled = radioButtonIndividualReport.Checked;
        }

        private void ScheduleReportDialog_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            if(DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            _settings.IndividualReport = radioButtonIndividualReport.Checked;
            _settings.SingleFile = checkBoxSingleFile.Checked;
            _settings.DetailLevel = DetailLevel;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                new PersonalSettingDataRepository(uow).PersistSettingValue(_settings);
                uow.PersistAll();
            }
        }

        private void radioButtonShiftsPerDay_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
