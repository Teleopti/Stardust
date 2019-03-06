using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public partial class ResourceCalculationAnalyzerView : Form
	{
		private readonly ResourceCalculationAnalyzerModel _model;
		private readonly Dictionary<int, int> _columnWidths = new Dictionary<int, int>();
		private int header = 210;
		private int narrow = 110;
		private int wide = 160;

		public ResourceCalculationAnalyzerView()
		{
			InitializeComponent();
		}

		public ResourceCalculationAnalyzerView(ResourceCalculationAnalyzerModel model)
		{			
			InitializeComponent();
			toolStripLabel1.Text = string.Empty;
			_model = model;
			_columnWidths.Add(0, header);
			_columnWidths.Add(1, narrow);
			_columnWidths.Add(2, narrow);
			_columnWidths.Add(3, wide);
			_columnWidths.Add(4, narrow);
			_columnWidths.Add(5, wide);
			_columnWidths.Add(6, narrow);
			_columnWidths.Add(7, narrow);
		}

		private void resetColumnWiths()
		{
			foreach (var i in _columnWidths)
			{
				listView1.Columns[i.Key].Width = i.Value;
			}
		}

		private void ResourceCalculationAnalyzerView_Load(object sender, EventArgs e)
		{
			dateTimePicker2.MinDate =  _model.Period().StartDate.Date;
			dateTimePicker2.MaxDate = _model.Period().EndDate.Date;
			dateTimePicker2.Value = _model.SelectedDate.Date;

			var selectedTime = _model.SelectedTime.HasValue
				? DateTime.Now.Date.AddTicks(_model.SelectedTime.Value.Ticks)
				: DateTime.Now;
			dateTimePicker1.Value = selectedTime;
			resetColumnWiths();
		}

		private void disableControls()
		{
			toolStrip1.Enabled = false;
			toolStripProgressBar1.Visible = true;
		}

		private void enableControls()
		{
			toolStrip1.Enabled = true;
			toolStripStatusLabel1.Text = "Ready";
			toolStripProgressBar1.Visible = false;
		}

		private void displayResult(IDictionary<ISkill, ResourceCalculationAnalyzerModel.ResourceCalculationAnalyzerModelResult> result)
		{
			var cascadingLevels = new List<int>();
			foreach (var skill in result.Keys)
			{
				var cascadingIndex = skill.CascadingIndex ?? -1;
				if (cascadingLevels.Contains(cascadingIndex))
					continue;

				cascadingLevels.Add(cascadingIndex);
			}

			cascadingLevels.Sort();
			foreach (var cascadingLevel in cascadingLevels)
			{
				listView1.Groups.Add(cascadingLevel.ToString(), cascadingLevel == -1 ? "Not Cascaded" : "Level " + cascadingLevel);
			}

			listView1.Groups.Add("summary", "Summary");

			var shoveledGainLossSummary = 0d;
			foreach (var pair in result)
			{
				var groupKey = (pair.Key.CascadingIndex ?? -1).ToString();
				var item = new ListViewItem(pair.Key.Name);
				item.Group = listView1.Groups[groupKey];
				item.SubItems.Add(Math.Round(pair.Value.Forecasted, 2).ToString());
				item.SubItems.Add(Math.Round(pair.Value.PrimaryResources, 2).ToString());
				item.SubItems.Add(Math.Round(pair.Value.PrimaryPercent.ValueAsPercent(), 1) + "%");
				item.SubItems.Add(Math.Round(pair.Value.ShoveledResources, 2).ToString());
				item.SubItems.Add(Math.Round(pair.Value.ShoveledPercent.ValueAsPercent(), 1) + "%");
				var shoveledGainLoss = pair.Value.ShoveledResources - pair.Value.PrimaryResources;
				shoveledGainLossSummary += shoveledGainLoss;
				item.SubItems.Add(Math.Round(shoveledGainLoss, 2).ToString());
				if (shoveledGainLoss > 0.00001)
					item.ForeColor = Color.Blue;
				if (shoveledGainLoss < -0.00001)
					item.ForeColor = Color.Red;
				item.SubItems.Add(Math.Round(pair.Value.Esl.ValueAsPercent(), 0) + "%");
				listView1.Items.Add(item);
			}

			var item1 = new ListViewItem("Totals");
			item1.Group = listView1.Groups["summary"];
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add(Math.Round(shoveledGainLossSummary, 2).ToString());
			if (shoveledGainLossSummary > 0.00001)
				item1.ForeColor = Color.CornflowerBlue;
			if (shoveledGainLossSummary < -0.00001)
				item1.ForeColor = Color.Red;
			listView1.Items.Add(item1);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			dateTimePicker1.Visible = false;
			dateTimePicker2.Visible = false;
			button1.Visible = false;
			toolStripLabel1.Text = dateTimePicker2.Value.Date.ToShortDateString() + " " + dateTimePicker1.Value.TimeOfDay;
			disableControls();
			backgroundWorker1.RunWorkerAsync();	
		}

		private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			e.Result = _model.AnalyzeLastChange(dateTimePicker2.Value.Date.AddTicks(dateTimePicker1.Value.TimeOfDay.Ticks), backgroundWorker1);
		}

		private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			toolStripStatusLabel1.Text = e.UserState as string;
			toolStripProgressBar1.Value = e.ProgressPercentage;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				var ex = new Exception("Background thread exception", e.Error);
				throw ex;
			}

			if (e.Result == null)
			{
				MessageBox.Show("Nothing to undo, please make at least one change", "Tjillevippen");
				Close();
			}

			var result = e.Result as IDictionary<ISkill, ResourceCalculationAnalyzerModel.ResourceCalculationAnalyzerModelResult>;
			if (result == null)
				return;

			displayResult(result);
			enableControls();
		}
	}
}
