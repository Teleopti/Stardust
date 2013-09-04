using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Win.Scheduling
{
    public partial class OptimizationProgress : Form
    {
        private DataSet _prodDs1;
        const string TableName = "Progress";

        public OptimizationProgress()
        {
            InitializeComponent();
        }

        private static void setupChartStyle(ChartControl chart)
        {

            chart.Palette = ChartColorPalette.Custom;
            chart.CustomPalette = new[] 
            {
                Color.FromArgb(200,0,255,0)
            };

            chart.BackInterior = new BrushInfo(Color.Black);
            chart.ChartInterior = new BrushInfo(Color.Black);
            chart.SmoothingMode = SmoothingMode.AntiAlias;
            chart.ChartArea.PrimaryXAxis.HidePartialLabels = true;

            chart.PrimaryXAxis.LineType.ForeColor = Color.Transparent;
            chart.PrimaryYAxis.LineType.ForeColor = Color.Transparent;
            chart.PrimaryXAxis.TickColor = Color.Transparent;
            chart.PrimaryYAxis.TickColor = Color.Transparent;
            chart.PrimaryXAxis.TitleColor = Color.White;
            chart.PrimaryYAxis.TitleColor = Color.White;
            chart.PrimaryXAxis.ForeColor = Color.White;
            chart.PrimaryYAxis.ForeColor = Color.White;

            chart.PrimaryXAxis.SmallTicksPerInterval = 2;
            chart.PrimaryXAxis.DrawMinorGrid = true;
            chart.PrimaryXAxis.DrawGrid = false;
            chart.PrimaryYAxis.DrawGrid = false;
            chart.PrimaryXAxis.MinorGridLineType.ForeColor = Color.LightGreen;
            chart.PrimaryYAxis.SmallTicksPerInterval = 3;
            chart.PrimaryYAxis.DrawMinorGrid = true;
            chart.PrimaryYAxis.MinorGridLineType.ForeColor = Color.LightGreen;
            chart.PrimaryXAxis.MinorGridLineType.DashStyle = DashStyle.Dot;
            chart.PrimaryYAxis.MinorGridLineType.DashStyle = DashStyle.Dot;

            chart.PrimaryYAxis.RangeType = ChartAxisRangeType.Set;
            chart.PrimaryYAxis.Range.Min = 0;
            chart.PrimaryYAxis.Range.Max = 60;
            chart.PrimaryYAxis.Range.Interval = 10;
            chart.PrimaryYAxis.Title = "Period value";
            chart.PrimaryXAxis.Title = "Time";
            chart.Font = new Font("Verdana", 7.0f, FontStyle.Regular);
            chart.PrimaryXAxis.LabelRotate = true;
            chart.PrimaryXAxis.LabelRotateAngle = 270;
            chart.Titles[0].ForeColor = Color.White;
            chart.Titles[0].Font = new Font("Vernada", 11, FontStyle.Bold);
            chart.Series[0].Style.Border.Width = 2;

        }

        private void initializeChartData()
        {
            chartControl1.Indexed = false;
            ChartSeries series = new ChartSeries();
            series.Name = "Progress";
            series.Text = series.Name;
            ChartDataBindModel model = new ChartDataBindModel(_prodDs1, TableName);
            model.XName = "Date";
            model.YNames = new[] { "Load" };
            series.SeriesModel = model;
            series.Type = ChartSeriesType.Spline;
            series.Style.DisplayShadow = false;
            chartControl1.Series.Add(series);
            chartControl1.PrimaryXAxis.ValueType = ChartValueType.DateTime;
            chartControl1.PrimaryXAxis.DateTimeFormat = "dd-hh:mm tt";
            chartControl1.PrimaryXAxis.RoundingPlaces = 12;
            chartControl1.PrimaryXAxis.RangeType = ChartAxisRangeType.Set;
            chartControl1.PrimaryXAxis.DateTimeRange = new ChartDateTimeRange(DateTime.Now, DateTime.Now.AddDays(1), 4, ChartDateTimeIntervalType.Hours);
        }

        private void createDataSet()
        {
            _prodDs1 = new DataSet();
            _prodDs1.Locale =
                TeleoptiPrincipal.Current.Regional.Culture;
            _prodDs1.Tables.Add(TableName);

            _prodDs1.Tables[TableName].Columns.Add("Date", typeof(DateTime));
            _prodDs1.Tables[TableName].Columns.Add("Load", typeof(double));

        }

        public void AddData(DateTime dateTime, double value)
        {
            value = value*100;
            if (_prodDs1 != null && _prodDs1.Tables.Count > 0)
            {
                DataRow drEmpty = _prodDs1.Tables[TableName].NewRow();
                _prodDs1.Tables[TableName].Rows.Add(drEmpty);
                int count = Math.Max(0, _prodDs1.Tables[TableName].Rows.Count - 1);

                _prodDs1.Tables[TableName].Rows[count]["Load"] = value;
                _prodDs1.Tables[TableName].Rows[count]["Date"] = dateTime;
                DateTime lastTime = (DateTime)_prodDs1.Tables[TableName].Rows[count]["Date"];
                if (count > 50)
                {
                    _prodDs1.Tables[TableName].Rows.RemoveAt(0);
                    count = Math.Max(0, _prodDs1.Tables[TableName].Rows.Count - 1);
                }
                    
                chartControl1.PrimaryXAxis.DateTimeRange =
                    new ChartDateTimeRange((DateTime)_prodDs1.Tables[TableName].Rows[0]["Date"], lastTime, 4,
                                           ChartDateTimeIntervalType.Auto);

                double maxValue = 0;
                double minValue = double.MaxValue;
                for (int i = 0; i < count + 1; i++)
                {
                    double thisValue = (double)_prodDs1.Tables[TableName].Rows[i]["Load"];
                    if (thisValue > maxValue)
                        maxValue = thisValue;
                    if (thisValue < minValue)
                        minValue = thisValue;
                }

                maxValue = maxValue + (maxValue*0.1);
                minValue = minValue - (minValue*0.1);

                chartControl1.PrimaryYAxis.Range = new MinMaxInfo(minValue, maxValue, (maxValue - minValue) / 10);
                //if (lastTime >= this.chartControl1.PrimaryXAxis.DateTimeRange.End)
                //    this.chartControl1.PrimaryXAxis.DateTimeRange = new ChartDateTimeRange(this.chartControl1.PrimaryXAxis.DateTimeRange.Start, this.chartControl1.PrimaryXAxis.DateTimeRange.End.AddDays(1), 4, ChartDateTimeIntervalType.Hours);
            }

        }

        private void OptimizationProgress_Load(object sender, EventArgs e)
        {
            createDataSet();
            setupChartStyle(chartControl1);
            initializeChartData();
        }
    }

    
}
