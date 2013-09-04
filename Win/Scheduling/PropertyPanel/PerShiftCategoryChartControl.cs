using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
    class PerShiftCategoryChartControl : BaseUserControl
    {
        private IDistributionInformationExtractor _model;
        private TableLayoutPanel tableLayoutPanel1;
        private readonly PerShiftCategoryChart _chart;
        private ComboBoxAdv _comboBoxShiftCategory;
        private Label label1;
       
        
        public PerShiftCategoryChartControl()
        {
            initializeComponent();
            _chart = new PerShiftCategoryChart();
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(_comboBoxShiftCategory,0,1);
            tableLayoutPanel1.Controls.Add(_chart, 0, 2);
        }

        private void selectedIndexChanged(object sender, EventArgs e)
        {
           
            updateChart();
        }

        public void UpdateModel(IDistributionInformationExtractor model)
        {
            _model = model;
            _comboBoxShiftCategory.Items.Clear();
            foreach (var shiftCategory in _model.ShiftCategories.OrderBy(s => s.Description.Name))
            {
                _comboBoxShiftCategory.Items.Add(shiftCategory.Description.Name);
            }
            if (_comboBoxShiftCategory.Items.Count > 0)
                _comboBoxShiftCategory.SelectedIndex = 0;
            updateChart();
        }

        private void updateChart()
        {
            IShiftCategory selectedShiftCategory = null;
            _chart.Series.Clear();
            var tempList =
                _model.ShiftCategories.Where(
                    shiftCategory => shiftCategory.Description.Name.Equals(_comboBoxShiftCategory.SelectedItem)).ToArray();
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
	            _chart.ShowLegend = true;
                var chartSeriesTemp = new ChartSeries(UserTexts.Resources.NoDataAvailable);
	            chartSeriesTemp.Style.Interior = new BrushInfo(_chart.BackColor);
                _chart.Model.Series.Add(chartSeriesTemp);
                chartSeriesTemp.Points.Add(0, 0);
				_chart.LegendAlignment = ChartAlignment.Center;
            }
            else
            {
				_chart.ShowLegend = false;
                var chartSeries = new ChartSeries(selectedShiftCategory.Description.Name);
                chartSeries.Style.Interior = new BrushInfo(selectedShiftCategory.DisplayColor);
                _chart.Model.Series.Add(chartSeries);
                Dictionary<int, int> frequency = _model.GetShiftCategoryFrequency(selectedShiftCategory);
                foreach (var item in frequency)
                {
                    chartSeries.Points.Add(item.Key, item.Value);
                }
            }    
        }

        private void initializeComponent()
        {
            label1 = new Label();
            _comboBoxShiftCategory = new ComboBoxAdv();
			_comboBoxShiftCategory.Margin = new Padding(33, 3, 3, 3);
            _comboBoxShiftCategory.SelectedIndexChanged += selectedIndexChanged;
            tableLayoutPanel1 = new TableLayoutPanel();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // PerShiftCategoryChartControl
            // 
            Controls.Add(tableLayoutPanel1);
            Name = "PerShiftCategoryChartControl";
            ResumeLayout(false);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(96, 13);
            label1.TabIndex = 0;
            label1.Text = UserTexts.Resources.PerShiftCategory;
	        label1.Margin = new Padding(30, 3, 3, 3);
        }     
    }
}
