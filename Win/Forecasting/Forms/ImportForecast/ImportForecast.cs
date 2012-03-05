using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
    public partial class ImportForecastForm : BaseRibbonForm, IImportForecast
    {
        private readonly ImportForecastPresenter _presenter;
        private ISkill _skill;
        private IWorkload _workload;

        public ImportForecastForm(ISkill preselectedSkill, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            InitializeComponent();
            _presenter = new ImportForecastPresenter(this, new ImportForecastModel(preselectedSkill, repositoryFactory, unitOfWorkFactory));
            GetSelectedSkillName();
            PopulateWorkloadList();
            _skill = preselectedSkill;
        }

        private void PopulateWorkloadList()
        {
            _presenter.PopulateWorkloadList();
            comboBoxAdvWorkloads.DataSource = _presenter.WorkloadList;
            comboBoxAdvWorkloads.DisplayMember = "Name";
        }

        private void GetSelectedSkillName()
        {
            _presenter.GetSelectedSkillName();
            txtSkillName.Text = _presenter.SkillName;
        }

        private void browseImportFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
         
            if (result == DialogResult.OK) // Test result.
            {
                textBoxImportFileName.Text = openFileDialog.FileName;
            }
        }


        public ISkill Skill
        {
            get { return _skill; }
            set { _skill = value; }
        }

        public IWorkload Workload
        {
            get { return (IWorkload)comboBoxAdvWorkloads.SelectedItem; }
            set { _workload = (IWorkload)comboBoxAdvWorkloads.SelectedItem; }
        }

        public bool IsWorkloadImport
        {
            get { return radioButtonImportWorkload.Checked; }
            set { radioButtonImportWorkload.Checked = value; }
        }

        public bool IsStaffingImport
        {
            get { return radioButtonImportStaffing.Checked; }
            set { radioButtonImportStaffing.Checked = value; }
        }

        public bool IsStaffingAndWorkloadImport
        {
            get { return radioButtonImportWLAndStaffing.Checked; }
            set { radioButtonImportWLAndStaffing.Checked = value; }
        }
    }
}
