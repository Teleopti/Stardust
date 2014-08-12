using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings.Overview
{
    public partial class ExportMeetingView : BaseDialogForm , IExportMeetingView
    {
        private readonly IEventAggregator _eventAggregator;

        public ExportMeetingView(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            
            InitializeComponent();
            SetTexts();
            comboBoxScenario.DisplayMember = "Description";
            comboBoxScenario.DropDownStyle = ComboBoxStyle.DropDownList;

			dateSelectionFromTo1.SetCulture(CultureInfo.CurrentCulture);
            dateSelectionFromTo1.ValueChanged += dateSelectionFromTo1ValueChanged;
        }

        void dateSelectionFromTo1ValueChanged(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<MeetingExportDatesChanged>().Publish(string.Empty);
        }
        
        private void buttonCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        public DialogResult ShowDialog(IMeetingOverviewView owner)
        {
            return ShowDialog((Form)owner);
        }

        public IList<DateOnlyPeriod> SelectedDates
        {
            get { return dateSelectionFromTo1.GetSelectedDates(); }
            set
            {
                dateSelectionFromTo1.WorkPeriodStart = value[0].StartDate;
                dateSelectionFromTo1.WorkPeriodEnd = value[0].EndDate;
                dateSelectionFromTo1.Refresh();
            }
        }

        public void SetScenarioList(IList<IScenario> scenarios)
        {
            comboBoxScenario.DataSource = scenarios;
        }

        public bool ExportEnabled
        {
            get { return buttonExport.Enabled; }
            set { buttonExport.Enabled = value; }
        }

        public void SetTextOnDateSelectionFromTo(string errorText)
        {
            dateSelectionFromTo1.SetErrorOnEndTime(errorText);
        }

        public IScenario SelectedScenario
        {
            get { return (IScenario) comboBoxScenario.SelectedItem; }
        }

        public string ExportInfoText
        {
            get { return labelExportResult.Text; }
            set { labelExportResult.Text = value; }
        }

        public bool ProgressBarVisible
        {
            get { return progressBarExporting.Visible; }
            set { progressBarExporting.Visible = value; }
        }

        public void ShowDataSourceException(DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.ExportMeetings,
                                                                    Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        private void buttonExportClick(object sender, EventArgs e)
        {
            _eventAggregator.GetEvent<ExportMeetingClicked>().Publish(string.Empty);
        }

		private void ExportMeetingView_Load(object sender, EventArgs e)
		{

		}

       
    }
}
