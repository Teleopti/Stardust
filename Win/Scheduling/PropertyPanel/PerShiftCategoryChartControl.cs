using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	class PerShiftCategoryChartControl : BaseUserControl, INeedShiftCategoryDistributionModel
    {
		private ComboBoxAdv comboBoxAdvActivity;
		private PerShiftCategoryChart perShiftCategoryChart1;
		private Label label1;
		private TableLayoutPanel tableLayoutPanel1;
        //private IDistributionInformationExtractor _model;
		//private TableLayoutPanel tableLayoutPanel1;
		//private readonly PerShiftCategoryChart _chart;
		//private ComboBoxAdv _comboBoxShiftCategory;
		//private Label label1;
		private IShiftCategoryDistributionModel _model;
       
        
        public PerShiftCategoryChartControl()
        {
            InitializeComponent();
			if(!DesignMode)
				SetTexts();
			//_chart = new PerShiftCategoryChart();
			//tableLayoutPanel1.Controls.Add(label1, 0, 0);
			//tableLayoutPanel1.Controls.Add(_comboBoxShiftCategory,0,1);
			//tableLayoutPanel1.Controls.Add(_chart, 0, 2);
        }

		//private void selectedIndexChanged(object sender, EventArgs e)
		//{
		//	updateChart();
		//}

		//public void UpdateModel(IDistributionInformationExtractor model)
		//{
		//	_model = model;
		//	_comboBoxShiftCategory.Items.Clear();
		//	foreach (var shiftCategory in _model.GetShiftCategories().OrderBy(s => s.Description.Name))
		//	{
		//		_comboBoxShiftCategory.Items.Add(shiftCategory.Description.Name);
		//	}
		//	if (_comboBoxShiftCategory.Items.Count > 0)
		//		_comboBoxShiftCategory.SelectedIndex = 0;
		//	updateChart();
		//}

		//private void updateChart()
		//{
		//	//IShiftCategory selectedShiftCategory = null;
		//	_chart.Series.Clear();
		//	//var tempList =
		//	//	_model.GetShiftCategories().Where(
		//	//		shiftCategory => shiftCategory.Description.Name.Equals(_comboBoxShiftCategory.SelectedItem)).ToArray();
		//	//if (tempList.Any())
		//	//{
		//	//	selectedShiftCategory = tempList.FirstOrDefault();
		//	//}
		//	//else
		//	//{
		//	//	assignValuesToChart(null);
		//	//}
		//	//if (selectedShiftCategory == null) return;

		//	//assignValuesToChart(selectedShiftCategory);
		//}

		//private void assignValuesToChart(IShiftCategory selectedShiftCategory)
		//{
			
		//	if (selectedShiftCategory == null)
		//	{
		//		_chart.ShowLegend = true;
		//		var chartSeriesTemp = new ChartSeries(UserTexts.Resources.NoDataAvailable);
		//		chartSeriesTemp.Style.Interior = new BrushInfo(_chart.BackColor);
		//		_chart.Model.Series.Add(chartSeriesTemp);
		//		chartSeriesTemp.Points.Add(0, 0);
		//		_chart.LegendAlignment = ChartAlignment.Center;
		//	}
		//	else
		//	{
		//		_chart.ShowLegend = false;
		//		var chartSeries = new ChartSeries(selectedShiftCategory.Description.Name);
		//		chartSeries.Style.Font.Facename = "Microsoft Sans Serif";
		//		chartSeries.Style.Interior = new BrushInfo(selectedShiftCategory.DisplayColor);
		//		_chart.Model.Series.Add(chartSeries);
		//		//Dictionary<int, int> frequency = _model.GetShiftCategoryFrequency(selectedShiftCategory);
		//		//frequency = addFakeValuesForEmptyPoints(frequency);
		//		//_chart.PrimaryXAxis.Range = new MinMaxInfo(0, frequency.Count, 1);

		//		//foreach (var item in frequency)
		//		//{
		//		//	chartSeries.Points.Add(item.Key, item.Value);    
		//		//}	
		//	}    
		//}

		//private Dictionary<int, int> addFakeValuesForEmptyPoints(IDictionary<int, int> frequency)
		//{
		//	var adjustededFrequency = new Dictionary<int, int>();
		//	var max = frequency.Select(keyValuePair => keyValuePair.Key).Concat(new[] {0}).Max();

		//	for (var i = 1; i <= max + 1; i++)
		//	{
		//		if (!frequency.ContainsKey(i))
		//		{
		//			adjustededFrequency.Add(i,0);
		//		}
		//		else
		//		{
		//			var value = frequency[i];
		//			adjustededFrequency.Add(i, value);
		//		}
		//	}

		//	return adjustededFrequency;
		//}

		//private void initializeComponent()
		//{
		//	//label1 = new Label();
		//	//_comboBoxShiftCategory = new ComboBoxAdv();
		//	//_comboBoxShiftCategory.Margin = new Padding(33, 3, 3, 3);
		//	//_comboBoxShiftCategory.SelectedIndexChanged += selectedIndexChanged;
		//	//_comboBoxShiftCategory.DropDownStyle = ComboBoxStyle.DropDownList;
		//	//_comboBoxShiftCategory.Style = VisualStyle.Office2007;
		//	//tableLayoutPanel1 = new TableLayoutPanel();
		//	//SuspendLayout();
		//	//// 
		//	//// tableLayoutPanel1
		//	//// 
		//	//tableLayoutPanel1.ColumnCount = 1;
		//	//tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
		//	//tableLayoutPanel1.Dock = DockStyle.Fill;
		//	//tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
		//	//tableLayoutPanel1.Name = "tableLayoutPanel1";
		//	//tableLayoutPanel1.RowCount = 3;
		//	//tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
		//	//tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
		//	//tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
		//	//tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
		//	//tableLayoutPanel1.TabIndex = 0;
		//	//// 
		//	//// PerShiftCategoryChartControl
		//	//// 
		//	//Controls.Add(tableLayoutPanel1);
		//	//Name = "PerShiftCategoryChartControl";
		//	//ResumeLayout(false);
		//	//// 
		//	//// label1
		//	//// 
		//	//label1.AutoSize = true;
		//	//label1.Location = new System.Drawing.Point(3, 0);
		//	//label1.Name = "label1";
		//	//label1.Size = new System.Drawing.Size(96, 13);
		//	//label1.TabIndex = 0;
		//	//label1.Text = UserTexts.Resources.PerShiftCategory;
		//	//label1.Margin = new Padding(30, 3, 3, 3);
		//}

		private void InitializeComponent()
		{
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAdvActivity = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.perShiftCategoryChart1 = new Teleopti.Ccc.Win.Scheduling.PropertyPanel.PerShiftCategoryChart();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvActivity, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.perShiftCategoryChart1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(258, 267);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// comboBoxAdvActivity
			// 
			this.comboBoxAdvActivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAdvActivity.Dock = System.Windows.Forms.DockStyle.Left;
			this.comboBoxAdvActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvActivity.Location = new System.Drawing.Point(3, 33);
			this.comboBoxAdvActivity.Name = "comboBoxAdvActivity";
			this.comboBoxAdvActivity.Size = new System.Drawing.Size(160, 21);
			this.comboBoxAdvActivity.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAdvActivity.TabIndex = 15;
			this.comboBoxAdvActivity.SelectedIndexChanged += new System.EventHandler(this.comboBoxAdvActivity_SelectedIndexChanged);
			// 
			// perShiftCategoryChart1
			// 
			this.perShiftCategoryChart1.BackInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(237)))), ((int)(((byte)(254))))), System.Drawing.Color.FromArgb(((int)(((byte)(177)))), ((int)(((byte)(204)))), ((int)(((byte)(234))))));
			this.perShiftCategoryChart1.ChartArea.BackInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.Transparent, System.Drawing.Color.Transparent);
			this.perShiftCategoryChart1.ChartArea.CursorLocation = new System.Drawing.Point(0, 0);
			this.perShiftCategoryChart1.ChartArea.CursorReDraw = false;
			this.perShiftCategoryChart1.ChartAreaMargins = new Syncfusion.Windows.Forms.Chart.ChartMargins(5, 5, 5, 5);
			this.perShiftCategoryChart1.ChartInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(165)))), ((int)(((byte)(194)))), ((int)(((byte)(229))))), System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(236)))), ((int)(((byte)(250))))));
			this.perShiftCategoryChart1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.perShiftCategoryChart1.ElementsSpacing = 1;
			this.perShiftCategoryChart1.IsWindowLess = false;
			// 
			// 
			// 
			this.perShiftCategoryChart1.Legend.Location = new System.Drawing.Point(166, 43);
			this.perShiftCategoryChart1.Localize = null;
			this.perShiftCategoryChart1.Location = new System.Drawing.Point(3, 63);
			this.perShiftCategoryChart1.Name = "perShiftCategoryChart1";
			this.perShiftCategoryChart1.PrimaryXAxis.Crossing = double.NaN;
			this.perShiftCategoryChart1.PrimaryXAxis.DrawGrid = false;
			this.perShiftCategoryChart1.PrimaryXAxis.ForceZero = true;
			this.perShiftCategoryChart1.PrimaryXAxis.LabelIntersectAction = Syncfusion.Windows.Forms.Chart.ChartLabelIntersectAction.Rotate;
			this.perShiftCategoryChart1.PrimaryXAxis.Margin = true;
			this.perShiftCategoryChart1.PrimaryXAxis.RangePaddingType = Syncfusion.Windows.Forms.Chart.ChartAxisRangePaddingType.None;
			this.perShiftCategoryChart1.PrimaryYAxis.Crossing = double.NaN;
			this.perShiftCategoryChart1.PrimaryYAxis.ForceZero = true;
			this.perShiftCategoryChart1.PrimaryYAxis.Margin = true;
			this.perShiftCategoryChart1.Size = new System.Drawing.Size(252, 201);
			this.perShiftCategoryChart1.TabIndex = 16;
			this.perShiftCategoryChart1.Text = "xxDistribution";
			this.perShiftCategoryChart1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			// 
			// 
			// 
			this.perShiftCategoryChart1.Title.Name = "Default";
			this.perShiftCategoryChart1.Titles.Add(this.perShiftCategoryChart1.Title);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 17;
			this.label1.Text = "xxShiftCategory";
			// 
			// PerShiftCategoryChartControl
			// 
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "PerShiftCategoryChartControl";
			this.Size = new System.Drawing.Size(258, 267);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvActivity)).EndInit();
			this.ResumeLayout(false);

		}

		public void SetModel(IShiftCategoryDistributionModel model)
		{
			_model = model;
			model.ResetNeeded -= modelResetNeeded;
			model.ResetNeeded += modelResetNeeded;
			model.UpdateChartNeeded += model_UpdateChartNeeded;
			Update();
			UpdateChart();
		}

		void model_UpdateChartNeeded(object sender, EventArgs e)
		{
			UpdateChart();
		}

		void modelResetNeeded(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new EventHandler<EventArgs>(modelResetNeeded), sender, e);
			}
			else
			{
				Update();
				UpdateChart();
			}
		}

		public void Update()
		{
			comboBoxAdvActivity.Items.Clear();
			
			//add with datasource instead
			foreach (var shiftCategory in _model.GetSortedShiftCategories())
			{
				comboBoxAdvActivity.Items.Add(shiftCategory.Description.Name);
			}
			if (comboBoxAdvActivity.Items.Count > 0)
				comboBoxAdvActivity.SelectedIndex = 0;
		}

		private void comboBoxAdvActivity_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateChart();
		}

		public void UpdateChart()
		{
			IShiftCategory selectedShiftCategory = null;
			perShiftCategoryChart1.Series.Clear();

			//check on id/equals instead of name
			var tempList = _model.GetSortedShiftCategories().Where(shiftCategory => shiftCategory.Description.Name.Equals(comboBoxAdvActivity.SelectedItem)).ToArray();
			
			if (tempList.Any())
			{
				selectedShiftCategory = tempList.FirstOrDefault();
			}
			else
			{
				assignValuesToChart(null);
			}
			if (selectedShiftCategory == null) return;

			assignValuesToChart(selectedShiftCategory);
		}

		private void assignValuesToChart(IShiftCategory selectedShiftCategory)
		{

			if (selectedShiftCategory == null)
			{
				perShiftCategoryChart1.ShowLegend = true;
				var chartSeriesTemp = new ChartSeries(UserTexts.Resources.NoDataAvailable);
				chartSeriesTemp.Style.Interior = new BrushInfo(perShiftCategoryChart1.BackColor);
				perShiftCategoryChart1.Model.Series.Add(chartSeriesTemp);
				chartSeriesTemp.Points.Add(0, 0);
				perShiftCategoryChart1.LegendAlignment = ChartAlignment.Center;
			}
			else
			{
				perShiftCategoryChart1.ShowLegend = false;
				var chartSeries = new ChartSeries(selectedShiftCategory.Description.Name);
				chartSeries.Style.Font.Facename = "Microsoft Sans Serif";
				chartSeries.Style.Interior = new BrushInfo(selectedShiftCategory.DisplayColor);
				perShiftCategoryChart1.Model.Series.Add(chartSeries);

				Dictionary<int, int> frequency = new Dictionary<int, int>();

				foreach (var sortedPerson in _model.GetSortedPersons(false))
				{
					var num = _model.ShiftCategoryCount(sortedPerson, selectedShiftCategory);

					if (!frequency.ContainsKey(num))
					{
						frequency.Add(num, 1);
					}
					else
					{
						frequency[num]++;
					}
				}
				

				//Dictionary<int, int> frequency = _model.GetShiftCategoryFrequency(selectedShiftCategory);
				frequency = addFakeValuesForEmptyPoints(frequency);
				perShiftCategoryChart1.PrimaryXAxis.Range = new MinMaxInfo(0, frequency.Count, 1);

				foreach (var item in frequency)
				{
					chartSeries.Points.Add(item.Key, item.Value);
				}	
			}
		}

		private Dictionary<int, int> addFakeValuesForEmptyPoints(IDictionary<int, int> frequency)
		{
			var adjustededFrequency = new Dictionary<int, int>();
			var max = frequency.Select(keyValuePair => keyValuePair.Key).Concat(new[] { 0 }).Max();

			for (var i = 0; i <= max + 1; i++)
			{
				if (!frequency.ContainsKey(i))
				{
					adjustededFrequency.Add(i, 0);
				}
				else
				{
					var value = frequency[i];
					adjustededFrequency.Add(i, value);
				}
			}

			return adjustededFrequency;
		}
    }
}
