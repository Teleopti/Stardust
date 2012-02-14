using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
    public partial class QuickForecastView : BaseRibbonForm, IQuickForecastView
    {
        public QuickForecastView()
        {
            InitializeComponent();

			dateSelectionFromToTarget.SetCulture(CultureInfo.CurrentCulture);
			dateSelectionFromToStatistics.SetCulture(CultureInfo.CurrentCulture);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode) return;

            Presenter.Initialize();
        }

        public QuickForecastPresenter Presenter { get; set; }

        public void SetWorkloadCollection(IEnumerable<WorkloadModel> workloadModels)
        {
            checkedListBoxWorkloads.Items.AddRange(workloadModels.ToArray());
        }

        public void SetScenarioCollection(IEnumerable<ScenarioModel> scenarioModels)
        {
            comboBoxScenario.DisplayMember = "Name";
            comboBoxScenario.DataSource = new List<ScenarioModel>(scenarioModels);
        }

        public void SetSelectedScenario(ScenarioModel scenario)
        {
            comboBoxScenario.SelectedItem = scenario;
        }

        public void SetStatisticPeriod(DateOnlyPeriod statisticPeriod)
        {
            dateSelectionFromToStatistics.WorkPeriodStart = statisticPeriod.StartDate;
            dateSelectionFromToStatistics.WorkPeriodEnd = statisticPeriod.EndDate;
        }

        public void SetTargetPeriod(DateOnlyPeriod targetPeriod)
        {
            dateSelectionFromToTarget.WorkPeriodStart = targetPeriod.StartDate;
            dateSelectionFromToTarget.WorkPeriodEnd = targetPeriod.EndDate;
        }

        public void AppendWorkInProgress(string status)
        {
            if (textBoxResult.InvokeRequired)
            {
                textBoxResult.BeginInvoke(new Action<string>(AppendWorkInProgress), status);
                return;
            }
            textBoxResult.AppendText(status);
            textBoxResult.AppendText("\n");
        }

        private void checkedListBoxWorkloads_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var currentWorkload = (WorkloadModel) checkedListBoxWorkloads.Items[e.Index];
            if (e.NewValue==CheckState.Checked)
            {
                Presenter.AddWorkload(currentWorkload);
            }
            else
            {
                Presenter.RemoveWorkload(currentWorkload);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            textBoxResult.Clear();
            if (!dateSelectionFromToStatistics.IsWorkPeriodValid ||
                !dateSelectionFromToTarget.IsWorkPeriodValid)
            {
                textBoxResult.Text = UserTexts.Resources.DateFromGreaterThanDateTo;
                return;
            }

            if (checkedListBoxWorkloads.CheckedItems.Count==0)
            {
                textBoxResult.Text = "xxAtLeastOneWorkloadMustBeSelected";
                return;
            }

            DisableInterface();
            Presenter.SetStatisticPeriod(dateSelectionFromToStatistics.GetSelectedDates()[0]);
            Presenter.SetTargetPeriod(dateSelectionFromToTarget.GetSelectedDates()[0]);

            backgroundWorkerAutoForecast.RunWorkerAsync();
        }

        private void DisableInterface()
        {
            groupBox1.Enabled = groupBox2.Enabled = groupBox3.Enabled = groupBox4.Enabled = false;
            buttonRun.Enabled = buttonCancel.Enabled = false;
        }

        private void EnableInterface()
        {
            groupBox1.Enabled = groupBox2.Enabled = groupBox3.Enabled = groupBox4.Enabled = true;
            buttonRun.Enabled = buttonCancel.Enabled = true;
        }

        private void backgroundWorkerAutoForecast_DoWork(object sender, DoWorkEventArgs e)
        {
            Presenter.ExecuteAutoForecast();
        }

        private void backgroundWorkerAutoForecast_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Error!=null)
            {
                throw e.Error;
            }

            EnableInterface();
            UncheckAllWorkloads();
        }

        private void UncheckAllWorkloads()
        {
            var itemCount = checkedListBoxWorkloads.Items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                checkedListBoxWorkloads.SetItemChecked(i, false);
            }
        }

        private void comboBoxScenario_SelectedIndexChanged(object sender, EventArgs e)
        {
            Presenter.SetScenario((ScenarioModel) comboBoxScenario.SelectedItem);
        }

        private void checkBoxUpdateStandardTemplates_CheckedChanged(object sender, EventArgs e)
        {
            Presenter.ToggleUpdateStandardTemplates(checkBoxUpdateStandardTemplates.Checked);
        }
    }
}
