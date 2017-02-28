using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Chart
{
    [Serializable]
    public class ChartSettings : SettingValue
    {
        private readonly IDictionary<string, IChartSeriesSetting> _definedRowSetting = new Dictionary<string, IChartSeriesSetting>();
        private readonly IList<string> _selectedRows = new List<string>();

        public IList<string> SelectedRows
        {
            get { return _selectedRows; }
        }

        public IChartSeriesSetting DefinedSetting(string key, IList<IChartSeriesSetting> defaultRowSetting)
        {
            IChartSeriesSetting seriesSetting;
            _definedRowSetting.TryGetValue(key, out seriesSetting);
            if (seriesSetting != null)
                return seriesSetting;

            seriesSetting = defaultRowSetting.FirstOrDefault(s => s.DisplayKey == key);
            if (seriesSetting != null)
            {
                _definedRowSetting.Add(key, (IChartSeriesSetting)seriesSetting.Clone());
                return _definedRowSetting[key];
            }

            Random random = new Random();
            Color randomColor = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
            seriesSetting = new ChartSeriesSetting(key, randomColor, ChartSeriesDisplayType.Line, false, AxisLocation.Left);
            _definedRowSetting.Add(key, seriesSetting);
            return seriesSetting;
        }

    }
}
