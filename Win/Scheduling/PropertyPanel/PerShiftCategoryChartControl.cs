using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private PerShiftCategoryChart _chart;
        private ComboBoxAdv _comboBoxShiftCategory;
        private Label label1;
        private Label label2;
        
        

        public PerShiftCategoryChartControl()
        {
            initializeComponent();
            _chart = new PerShiftCategoryChart {Dock = DockStyle.Fill};
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(_comboBoxShiftCategory,0,1);
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            tableLayoutPanel1.Controls.Add(_chart, 0, 3);
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
            var tempList =
                _model.ShiftCategories.Where(
                    shiftCategory => shiftCategory.Description.Name.Equals(_comboBoxShiftCategory.SelectedItem)).ToArray();
            if (tempList.Any())
            {
                selectedShiftCategory = tempList.FirstOrDefault();
            }
            if (selectedShiftCategory == null) return;
            _chart.Series.Clear();
            var chartSeries = new ChartSeries(selectedShiftCategory.Description.Name);
            _chart.Model.Series.Add(chartSeries );
            Dictionary<int, int> frequency = _model.GetShiftCategoryFrequency(selectedShiftCategory);
            foreach (var item in frequency)
            {
                chartSeries.Points.Add(item.Key, item.Value);
            }
        }

        private void initializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            _comboBoxShiftCategory = new ComboBoxAdv();
            _comboBoxShiftCategory.Left = 400;
            _comboBoxShiftCategory.SelectedIndexChanged += new EventHandler(selectedIndexChanged);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(150, 150);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // PerShiftCategoryChartControl
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PerShiftCategoryChartControl";
            this.ResumeLayout(false);

            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(96, 13);
            label1.TabIndex = 0;
            label1.Text = UserTexts.Resources.PerShiftCategory;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 47);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(77, 13);
            label2.TabIndex = 1;
            label2.Text = UserTexts.Resources.OverView;

        }

        
    }
}
