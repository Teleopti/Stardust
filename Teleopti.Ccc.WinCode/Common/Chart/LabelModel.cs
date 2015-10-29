using Syncfusion.Windows.Forms.Chart;

namespace Teleopti.Ccc.WinCode.Common.Chart
{
    public class LabelModel : IChartAxisLabelModel
    {
        private readonly string[] _labels;
        private readonly string[] _tooltipLabels;
        
        public LabelModel(string[] labels)
        {
            _labels = labels;
            _tooltipLabels = labels;
        }

        public LabelModel(string[] labels, int maxLabels)
        {
            _labels = (string[]) labels.Clone();
            _tooltipLabels = (string[]) labels.Clone();
            if(_labels.Length > maxLabels)
            {
                //Blank every other 
                for (int i=0;i<_labels.Length;i++)
                {
                    if(i%2!=1) _labels[i] = string.Empty;
                }
            }
            
        }

        public ChartAxisLabel GetLabelAt(int index)
        {
            return new ChartAxisLabel(_labels[index]);
        }

        public ChartAxisLabel GetToolTipLabelAt(int index)
        {
            return new ChartAxisLabel(_tooltipLabels[index]);
        }

        public int Count
        {
            get
            {
                return _labels.GetLength(0);
            }
        }
    }
}