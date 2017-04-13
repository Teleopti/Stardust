using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common
{
    public partial class ModifySelectionView : BaseDialogForm, IModifySelectionView
    {
        private readonly ModifySelectionPresenter _presenter;
        private string _percentage;
        private const double maxValue = 999999999d;
        private const double minValue = -999999999d;

        public ModifySelectionView(ModifyCalculator model)
        {
            InitializeComponent();

            _presenter = new ModifySelectionPresenter(this, model);
            _presenter.Initialize();
            _percentage = textBoxExtPercent.Text;
            SetTexts();
            UpdateChart(true);
            chartControlSelection.BackInterior = ColorHelper.ChartControlBackInterior();
            chartControlSelection.ChartInterior = ColorHelper.ChartControlChartInterior();
        }

        private void UpdateChart(bool modified)
        {
            chartControlSelection.Series.Clear();
            var chartListModified = _presenter.Result();
            if (!modified) chartListModified = _presenter.OriginalResult();
            MakeSeries(chartListModified, Resources.ModifiedValues);
            if (checkBoxAdvShowOriginal.Checked)
            {
                MakeSeries(_presenter.OriginalResult(), Resources.OriginalValues);
            }
            chartControlSelection.ShowToolTips = true;
            chartControlSelection.PrimaryXAxis.Range.Max = chartListModified.Count+1;
        }

        void MakeSeries(IList<double> list, string text)
        {
            var seriesOriginal = new ChartSeries();
            var chartList = list;
            for (var i = 0; i < chartList.Count; i++)
            {
                seriesOriginal.Points.Add(i + 1, (int)chartList[i]);
                seriesOriginal.Styles[i].ToolTip = Math.Round(chartList[i], 1).ToString(CultureInfo.CurrentCulture);
            }
            seriesOriginal.PointsToolTipFormat = "{2}";
            seriesOriginal.Text = text;
            chartControlSelection.Series.Add(seriesOriginal);
        }

        public int InputState { get; set; }

        public double Sum
        {
            get { return Convert.ToDouble(autoLabelSum.Text, CultureInfo.CurrentCulture); }
            set { autoLabelSum.Text = value.ToString(CultureInfo.CurrentCulture); }
        }

        public double ChosenAmount
        {
            get { return Convert.ToDouble(autoLabelChosenAmount.Text, CultureInfo.CurrentCulture); }
            set { autoLabelChosenAmount.Text = value.ToString(CultureInfo.CurrentCulture); }
        }

        public double Average
        {
            get { return Convert.ToDouble(autoLabelAverage.Text, CultureInfo.CurrentCulture); }
            set { autoLabelAverage.Text = value.ToString(CultureInfo.CurrentCulture); }
        }

        public double StandardDev
        {
            get { return Convert.ToDouble(autoLabelStandardDev.Text, CultureInfo.CurrentCulture); }
            set { autoLabelStandardDev.Text = value.ToString(CultureInfo.CurrentCulture); }
        }

        public double ModifiedSum
        {
            get { return Convert.ToDouble(autoLabelResult.Text, CultureInfo.CurrentCulture); }
            set { autoLabelResult.Text = Math.Round(value, 1).ToString(CultureInfo.CurrentCulture); }
        }

        public string InputPercent
        {
            get { return _percentage; }
            set
            { if (_percentage != value) { _percentage = value;
            textBoxExtPercent.Text = _percentage;
            } }
        }

        public string InputType
        {
            get { return comboBoxInputType.SelectedIndex.ToString(CultureInfo.CurrentCulture); }
            set { comboBoxInputType.SelectedIndex = Convert.ToInt32(value, CultureInfo.CurrentCulture); } 
        }

        public string InputSmoothValue
        {
            get { return comboBoxSmoothen.Text; }
            set { comboBoxSmoothen.SelectedIndex = Convert.ToInt32(value, CultureInfo.CurrentCulture); } 
        }

        public IList<double> ModifiedList
        {
            get { return _presenter.Result(); }
        }

        public void SetDialogResult(DialogResult value)
        {
            DialogResult = value;
        }

        private void buttonAdvUndo_Click(object sender, EventArgs e)
        {
            _presenter.Undo();
            comboBoxSmoothen.SelectedIndex = 0;
            UpdateChart(false);
        }

        private void buttonAdvOK_Click(object sender, EventArgs e)
        {
            _presenter.Accept();    
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            _presenter.Cancel();
        }

        private void textBoxExtPercent_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (double.TryParse(textBoxExtPercent.Text, out value))
            {
                if (double.IsInfinity(value) || double.IsNaN(value))
                    textBoxExtPercent.Text = "0";
                if (InputType == "0")
                {
                    if (value > maxValue || value < minValue)
                        textBoxExtPercent.Text = _percentage;
                    else
                        _percentage = textBoxExtPercent.Text;
                }
                else
                {
                    if ((1 + value / 100) * Sum > maxValue || (1 + value / 100) * Sum < minValue)
                        textBoxExtPercent.Text = _percentage;
                    else
                        _percentage = textBoxExtPercent.Text;
                }
            }
            _presenter.UpdateInputTotal();
            UpdateChart(true);
        }

        private void comboBoxSmoothen_TextChanged(object sender, EventArgs e)
        {
            _presenter.UpdateSmoothList();// Creating first series
            UpdateChart(true);
        }

        private void comboBoxInputType_TextChanged(object sender, EventArgs e)
        {
                autoLabelPlusMinus.Visible = comboBoxInputType.SelectedIndex != 0;
                if (InputState.ToString(CultureInfo.CurrentCulture) == comboBoxInputType.SelectedIndex.ToString(CultureInfo.CurrentCulture)) return;
                InputType = comboBoxInputType.SelectedIndex.ToString(CultureInfo.CurrentCulture);
                _presenter.UpdateNewSum();
        }

        private void checkBoxAdvShowOriginal_CheckStateChanged(object sender, EventArgs e)
        {
            UpdateChart(true);
        }
    }
}
