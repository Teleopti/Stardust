using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class ResourceCalculationAnalyzerView : Form
	{
		private readonly ResourceCalculationAnalyzerModel _model;

		public ResourceCalculationAnalyzerView()
		{
			InitializeComponent();
		}

		public ResourceCalculationAnalyzerView(ResourceCalculationAnalyzerModel model)
		{			
			InitializeComponent();
			_model = model;
		}

		private void ResourceCalculationAnalyzerView_Load(object sender, EventArgs e)
		{
			dateTimePicker2.MinDate =  _model.Period().StartDate.Date;
			dateTimePicker2.MaxDate = _model.Period().EndDate.Date;
			dateTimePicker2.Value = dateTimePicker2.MinDate;
			dateTimePicker1.Value = DateTime.Now;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			dateTimePicker1.Visible = false;
			dateTimePicker2.Visible = false;
			button1.Visible = false;
			label1.Text = dateTimePicker2.Value.Date.ToShortDateString() + " " + dateTimePicker1.Value.TimeOfDay;
			var result = _model.AnalyzeLastChange(dateTimePicker2.Value.Date.AddTicks(dateTimePicker1.Value.TimeOfDay.Ticks));
			var cascadingLevels = new List<int>();
			foreach (var skill in result.Keys)
			{
				var cascadingIndex = skill.CascadingIndex ?? -1;
				//string cascadingKey = cascadingIndex.ToString();
				if(cascadingLevels.Contains(cascadingIndex))
					continue;

				cascadingLevels.Add(cascadingIndex);			
			}

			cascadingLevels.Sort();
			foreach (var cascadingLevel in cascadingLevels)
			{
				listView1.Groups.Add(cascadingLevel.ToString(), cascadingLevel == -1 ? "Not Cascaded" : "Level " + cascadingLevel);
			}

			listView1.Groups.Add("summary", "Summary");

			var primaryGainLossSummary = 0d;
			var shoveledGainLossSummary = 0d;
			foreach (var pair in result)
			{
				var groupKey = (pair.Key.CascadingIndex ?? -1).ToString();
				var item = new ListViewItem(pair.Key.Name);
				item.Group = listView1.Groups[groupKey];
				item.SubItems.Add(Math.Round(pair.Value.PrimaryPercentBefore.ValueAsPercent(), 2) + "%");
				item.SubItems.Add(Math.Round(pair.Value.PrimaryPercentAfter.ValueAsPercent(), 2) + "%");
				item.SubItems.Add(Math.Round(pair.Value.PrimaryResourcesBefore, 2).ToString());
				item.SubItems.Add(Math.Round(pair.Value.PrimaryResourcesAfter, 2).ToString());
				var primaryGainLoss = pair.Value.PrimaryResourcesAfter - pair.Value.PrimaryResourcesBefore;
				primaryGainLossSummary += primaryGainLoss;
				item.SubItems.Add(Math.Round(primaryGainLoss, 2).ToString());
				if(primaryGainLoss > 0.00001)
					item.ForeColor = Color.CornflowerBlue;
				if (primaryGainLoss < -0.00001)
					item.ForeColor = Color.Red;
				item.SubItems.Add(Math.Round(pair.Value.ShoveledPercentBefore.ValueAsPercent(), 2) + "%");
				item.SubItems.Add(Math.Round(pair.Value.ShoveledPercentAfter.ValueAsPercent(), 2) + "%");
				item.SubItems.Add(Math.Round(pair.Value.ShoveledResourcesBefore, 2).ToString());
				item.SubItems.Add(Math.Round(pair.Value.ShoveledResourcesAfter, 2).ToString());
				var shoveledGainLoss = pair.Value.ShoveledResourcesAfter - pair.Value.ShoveledResourcesBefore;
				shoveledGainLossSummary += shoveledGainLoss;
				item.SubItems.Add(Math.Round(shoveledGainLoss, 2).ToString());
				if (primaryGainLoss > 0.00001)
					item.ForeColor = Color.CornflowerBlue;
				if (primaryGainLoss < -0.00001)
					item.ForeColor = Color.Red;
				listView1.Items.Add(item);
			}

			var item1 = new ListViewItem("Totals");
			item1.Group = listView1.Groups["summary"];
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add("");
			item1.SubItems.Add(Math.Round(primaryGainLossSummary, 2).ToString());
			if (primaryGainLossSummary > 0.00001)
				item1.ForeColor = Color.CornflowerBlue;
			if (primaryGainLossSummary < -0.00001)
				item1.ForeColor = Color.Red;

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
	}
}
