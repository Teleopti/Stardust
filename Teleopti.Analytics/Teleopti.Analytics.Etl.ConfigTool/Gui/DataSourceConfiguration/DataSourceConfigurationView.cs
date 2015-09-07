using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration;
using Teleopti.Analytics.Etl.ConfigTool.Gui.StartupConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.DataSourceConfiguration
{
    public partial class DataSourceConfigurationView : Form, IDataSourceConfigurationView
    {
	    private readonly string _connectionString;
	    private readonly DataSourceConfigurationPresenter _presenter;
        private bool _showForm;
        private bool _closeApplication;
        private IJob _initialJob;

        public event EventHandler<AlarmEventArgs> TimeToStartInitialLoad;

        private DataSourceConfigurationView() { }

		  public DataSourceConfigurationView(DataSourceConfigurationModel model, string connectionString)
            : this()
        {
			  _connectionString = connectionString;
			  InitializeComponent();
            _presenter = new DataSourceConfigurationPresenter(this, model);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void Initialize()
        {
            _showForm = true;
            _closeApplication = false;


			var configurationHandler = new ConfigurationHandler(new GeneralFunctions(_connectionString));

			if (!configurationHandler.IsConfigurationValid)
			{
				var startupConfigurationView = new StartupConfigurationView(configurationHandler);
				startupConfigurationView.ShowDialog();
			}

			if (configurationHandler.BaseConfiguration.CultureId != null)
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(configurationHandler.BaseConfiguration.CultureId.Value).FixPersianCulture();



			//MessageBoxProperties msgBoxProps = _presenter.CheckForFirstUse();
   //         if (msgBoxProps != null)
   //         {
   //             // Either it´s the first time ETL Tool is used or it tis in an invalid state - show msgbox.
   //             MessageBoxButtons messageBoxButtons = MessageBoxButtons.YesNo;
   //             MessageBoxIcon messageBoxIcon = MessageBoxIcon.Question;

   //             if (!msgBoxProps.IsQuestion)
   //             {
   //                 // ETL Tool is in an invalid state
   //                 messageBoxButtons = MessageBoxButtons.OK;
   //                 messageBoxIcon = MessageBoxIcon.Error;
   //             }

   //             DialogResult dialogResult = MessageBox.Show(msgBoxProps.Text, msgBoxProps.Caption, messageBoxButtons,
   //                                                         messageBoxIcon);
   //             if (dialogResult == DialogResult.No || dialogResult == DialogResult.OK)
   //             {
   //                 _showForm = false;
   //                 _closeApplication = true;
   //                 return;
   //             }
   //         }
            _presenter.Initialize();
        }

        private void DataSourceConfigurationView_Load(object sender, EventArgs e)
        {
            initializeGrid();
            _presenter.SetGridState();
        }

        private void initializeGrid()
        {
            dataGridView1.CellValueChanged += new DataGridViewCellEventHandler(dataGridView1_CellValueChanged);

            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dataGridView1.MultiSelect = false;

            // Columns
        	var statusColumn = new DataGridViewImageColumn
        	                   	{
        	                   		Name = "Status",
        	                   		HeaderText = DataSourceStatusColumnHeader,
        	                   		ReadOnly = true,
        	                   		AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        	                   	};

        	var nameColumn = new DataGridViewTextBoxColumn
        	                 	{
        	                 		DataPropertyName = "Name",
        	                 		HeaderText = DataSourceNameColumnHeader,
        	                 		ReadOnly = true,
        	                 		AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        	                 	};

        	var timeZoneComboboxColumn = new DataGridViewComboBoxColumn
        	                             	{
        	                             		Name = "TimeZone",
        	                             		DataSource = TimeZoneDataSource,
        	                             		ValueMember = "TimeZoneCode",
        	                             		DisplayMember = "TimeZoneName",
        	                             		DataPropertyName = "TimeZoneCode",
        	                             		HeaderText = DataSourceTimeZoneColumnHeader,
        	                             		AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        	                             	};

        	var intervalLengthColumn = new DataGridViewTextBoxColumn
        	                           	{
        	                           		DataPropertyName = "IntervalLengthText",
        	                           		HeaderText = DataSourceIntervalLengthColumnHeader,
        	                           		ReadOnly = true,
        	                           		AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        	                           	};

        	var activeColumn = new DataGridViewCheckBoxColumn
        	                   	{
        	                   		DataPropertyName = "Inactive",
        	                   		HeaderText = DataSourceInactiveColumnHeader,
        	                   		AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        	                   	};

        	dataGridView1.Columns.Add(statusColumn);
            dataGridView1.Columns.Add(nameColumn);
            dataGridView1.Columns.Add(timeZoneComboboxColumn);
            dataGridView1.Columns.Add(intervalLengthColumn);
            dataGridView1.Columns.Add(activeColumn);

            dataGridView1.DataSource = DataSource;
        }

        public bool ShowForm
        {
            get
            {
                return _showForm;
            }
        }

        public bool CloseApplication
        {
            get { return _closeApplication; }
            set { _closeApplication = value; }
        }

        public string DataSourceStatusColumnHeader { get; set; }
        public string DataSourceNameColumnHeader { get; set; }
        public string DataSourceTimeZoneColumnHeader { get; set; }
        public string DataSourceIntervalLengthColumnHeader { get; set; }
        public string DataSourceInactiveColumnHeader { get; set; }
        public IList<DataSourceRow> DataSource { get; private set; }
        public IList<ITimeZoneDim> TimeZoneDataSource { get; private set; }

        public void SetDataSource(IList<DataSourceRow> dataSource)
        {
            DataSource = dataSource;
        }

        public void SetTimeZoneDataSource(IList<ITimeZoneDim> dataSource)
        {
            TimeZoneDataSource = dataSource;
        }


        public void SetOkButtonEnabled(bool isEnabled)
        {
            buttonOk.Enabled = isEnabled;
        }

        public void SetViewEnabled(bool isEnabled)
        {
            buttonOk.Enabled = isEnabled;
            buttonCancel.Enabled = isEnabled;
            dataGridView1.Enabled = isEnabled;
        }

        public void SetRowStateImage(DataSourceRow dataSourceRow)
        {
            DataGridViewRow row = dataGridView1.Rows[dataSourceRow.RowIndex];
            var imageCell = (DataGridViewImageCell)row.Cells["Status"];
            switch (dataSourceRow.RowState)
            {
                case DataSourceState.Valid:
                    imageCell.Value = Properties.Resources.ok_16x16;
                    break;
                case DataSourceState.Invalid:
                    imageCell.Value = Properties.Resources.warning_16x16;
                    break;
                case DataSourceState.Error:
                    imageCell.Value = Properties.Resources.error_16x16;
                    break;
            }
            imageCell.ToolTipText = dataSourceRow.RowStateToolTip;
        }

        public void SetRowReadOnly(DataSourceRow dataSourceRow)
        {
            DataGridViewRow row = dataGridView1.Rows[dataSourceRow.RowIndex];

            if (!dataSourceRow.IsRowEnabled)
            {
                row.ReadOnly = true;
                row.DefaultCellStyle.BackColor = BackColor;
                row.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }
        }

        public void SetToolStripState(bool showSpinningProgress, string message)
        {
            toolStripSpinningProgressControl1.Visible = showSpinningProgress;
            toolStripStatusLabel1.Text = message;

        }

        public void RunInitialJob()
        {
            TimeToStartInitialLoad(new object(), new AlarmEventArgs(_initialJob));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        public void ShowErrorMessage(string message)
        {
            Cursor = Cursors.Default;
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void CloseView()
        {
            Cursor = Cursors.Default;
            Close();
        }

        public void EtlToolIsNowReady(IJob initialJob)
        {
            // When the manual ETL Tool is logged into Raptor´s domain and all jobs are loaded and ready to rock.
            // The "Initial" job is injected
            // When we are here this window is ready to save - enable OK button if appropriate.
            _presenter.SetViewReadyToSave(initialJob);
        }

        public bool IsEtlToolLoading { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetInitialJob(IJob job)
        {
            _initialJob = job;
            _initialJob.JobExecutionReady += new EventHandler<AlarmEventArgs>(initialJob_JobExecutionReady);
        }

        void initialJob_JobExecutionReady(object sender, AlarmEventArgs e)
        {
            _presenter.Save(e.Job);
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        public bool IsSaved { get; set; }
        public void SetTimeZoneSelected(DataSourceRow dataSourceRow)
        {
            dataGridView1[2, dataSourceRow.RowIndex].Value = dataSourceRow.TimeZoneCode;
            dataGridView1.Refresh();
        }

        public void SetTimeZoneComboState(int rowIndex, bool isEnabled)
        {
            var comboBoxCell = dataGridView1.Rows[rowIndex].Cells["TimeZone"] as DataGridViewComboBoxCell;
            if (comboBoxCell != null)
            {
                comboBoxCell.ReadOnly = !isEnabled;
            }
        }

        public void DetachEvent()
        {
            if (_initialJob != null)
                _initialJob.JobExecutionReady -= initialJob_JobExecutionReady;
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            //if (dataGridView1.CurrentCell is DataGridViewComboBoxCell || dataGridView1.CurrentCell is DataGridViewCheckBoxCell)
            //{
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            //}
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
            var comboBoxCell = cell as DataGridViewComboBoxCell;
            var checkBoxCell = cell as DataGridViewCheckBoxCell;
            var dataSourceRow = (DataSourceRow)dataGridView1.Rows[e.RowIndex].DataBoundItem;

            if (comboBoxCell != null)
            {
                _presenter.SetRowState(dataSourceRow);
            }
            if (checkBoxCell != null)
            {
                _presenter.SetRowState(dataSourceRow);
                _presenter.SetSaveState();
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _presenter.InitiateSave(_initialJob);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _presenter.CancelView();
        }
    }
}