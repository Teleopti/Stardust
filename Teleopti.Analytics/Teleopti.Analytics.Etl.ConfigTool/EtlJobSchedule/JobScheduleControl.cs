using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Analytics.Etl.Common.Database;
using Teleopti.Analytics.Etl.Common.Database.EtlSchedules;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule
{
    public partial class JobScheduleControl : UserControl
    {
        public JobScheduleControl()
        {
            InitializeComponent();
        }


        private String _connectionString;
        private IScheduleRepository _repository;
        private IEtlScheduleCollection etlScheduleCollection;



        private void JobSheduleControl_Load(object sender, EventArgs e)
        {
            _connectionString = ConfigurationManager.AppSettings["stageConnectionString"];

            if (_connectionString != null)
            {
                _repository = new Repository(_connectionString);

                InitializeGrid();
                UpdateGridGrid(0);
            }
        }

        private void InitializeGrid()
        {
            dataGridViewJobSchedules.ReadOnly = true;
            dataGridViewJobSchedules.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewJobSchedules.AllowUserToResizeRows = false;
            dataGridViewJobSchedules.AllowUserToAddRows = false;
            dataGridViewJobSchedules.AllowUserToDeleteRows = false;
            dataGridViewJobSchedules.AllowUserToOrderColumns = false;
            dataGridViewJobSchedules.AllowUserToResizeColumns = true;
            dataGridViewJobSchedules.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewJobSchedules.MultiSelect = false;
        }

        private void UpdateGridGrid(int selectIndex)
        {
            // Load job schedules
            etlScheduleCollection = new EtlScheduleCollection(_repository, null, DateTime.Now);

            dataGridViewJobSchedules.Rows.Clear();

            foreach (IEtlSchedule schedule in etlScheduleCollection)
            {
                string[] row = { schedule.ScheduleName, schedule.JobName, schedule.Enabled.ToString(), schedule.Description };
                dataGridViewJobSchedules.Rows.Add(row);
                dataGridViewJobSchedules.Rows[dataGridViewJobSchedules.Rows.Count - 1].Tag = schedule;
            }

            if (dataGridViewJobSchedules.Rows.Count > 0)
            {
                dataGridViewJobSchedules.Rows[selectIndex].Selected = true;
            }
        }

        private void dataGridViewJobSchedules_SelectionChanged(object sender, EventArgs e)
        {
            ChangeButtonState();
        }

        private void ChangeButtonState()
        {
            if (dataGridViewJobSchedules.SelectedRows.Count > 0)
            {
                toolStripButtonEdit.Enabled = true;
                toolStripButtonRemove.Enabled = true;
            }
            else
            {
                toolStripButtonEdit.Enabled = false;
                toolStripButtonRemove.Enabled = false;
            }
        }

        private void toolStripButtonEdit_Click(object sender, EventArgs e)
        {
            OpenJobSchedule(((IEtlSchedule)dataGridViewJobSchedules.SelectedRows[0].Tag).ScheduleId);
        }

        private void dataGridViewJobSchedules_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridViewJobSchedules.SelectedRows.Count > 0)
            {
                toolStripButtonEdit_Click(sender, e);
            }
        }

        private void toolStripButtonRemove_Click(object sender, EventArgs e)
        {
            if (dataGridViewJobSchedules.SelectedRows.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show(this, "Are you sure you want to remove the job schedule?",
                                                            "Confirm",
                                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                            MessageBoxDefaultButton.Button2,
                                                            (RightToLeft == RightToLeft.Yes)
                                                                ? MessageBoxOptions.RtlReading |
                                                                  MessageBoxOptions.RightAlign
                                                                : 0);
                if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    _repository.DeleteSchedule(((IEtlSchedule)dataGridViewJobSchedules.SelectedRows[0].Tag).ScheduleId);

                    int index = -1;
                    if (dataGridViewJobSchedules.SelectedRows[0].Index == dataGridViewJobSchedules.Rows.Count - 1)
                    {
                        if (dataGridViewJobSchedules.Rows.Count > 1)
                        {
                            //The row at the bottom of the grid is being removed
                            index = dataGridViewJobSchedules.SelectedRows[0].Index - 1;
                        }
                    }
                    else
                    {
                        //A row at the top or in the middle of the grid is being removed
                        index = dataGridViewJobSchedules.SelectedRows[0].Index;
                    }
                    UpdateGridGrid(index);
                }
            }
        }

        private void toolStripButtonNew_Click(object sender, EventArgs e)
        {
            OpenJobSchedule(-1);
        }

        private void OpenJobSchedule(int scheduleId)
        {
            JobSchedule jobSchedule = new JobSchedule(scheduleId);
            if (jobSchedule.ShowDialog() == DialogResult.OK)
            {
                int index = -1;
                if (dataGridViewJobSchedules.SelectedRows.Count > 0)
                {
                    if (scheduleId == -1)
                    {
                        index = dataGridViewJobSchedules.Rows.Count - 1;
                    }
                    else
                    {
                        index = dataGridViewJobSchedules.SelectedRows[0].Index;
                    }
                }
                UpdateGridGrid(index);
            }
        }

    




    }
}
