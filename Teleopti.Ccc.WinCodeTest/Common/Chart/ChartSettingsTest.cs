using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Chart
{
    [TestFixture]
    public class ChartSettingsTest
    {
        private ChartSettings _target;

        [SetUp]
        public void Setup()
        {
            _target = new ChartSettings();
        }

        [Test]
        public void VerifyRandomRowSettingIsReturned()
        {
            IChartSeriesSetting rowSetting = _target.DefinedSetting("kalle", new List<IChartSeriesSetting>());
            Assert.IsNotNull(rowSetting);
        }

        [Test]
        public void VerifyDefaultIsReturnedIfNoDefined()
        {
            IList<IChartSeriesSetting> defaults = new List<IChartSeriesSetting>();
            defaults.Add(new ChartSeriesSetting("kalle", Color.Firebrick, ChartSeriesDisplayType.Bar, false, AxisLocation.Right));
            IChartSeriesSetting rowSetting = _target.DefinedSetting("kalle", defaults);
            Assert.AreEqual(Color.Firebrick, rowSetting.Color);
            //Now the value should exist in defined
            rowSetting = _target.DefinedSetting("kalle", new List<IChartSeriesSetting>());
            Assert.AreEqual(Color.Firebrick, rowSetting.Color);
        }

        [Test]
        public void VerifyDefinedSettingIsClonedFromDefault()
        {
            List<IChartSeriesSetting> defaults = new List<IChartSeriesSetting>();
            defaults.Add(new ChartSeriesSetting("kalle", Color.Firebrick, ChartSeriesDisplayType.Bar, false, AxisLocation.Right));
            IChartSeriesSetting rowSetting = _target.DefinedSetting("kalle", defaults);
            rowSetting.Color = Color.ForestGreen;
            Assert.AreEqual(Color.Firebrick, defaults[0].Color);
            rowSetting = _target.DefinedSetting("kalle", defaults);
            Assert.AreEqual(Color.ForestGreen, rowSetting.Color);
        }

        [Test]
        public void VerifySelectedRowKeys()
        {
            Assert.AreEqual(0, _target.SelectedRows.Count);
            _target.SelectedRows.Add("kalle");
            Assert.AreEqual(1, _target.SelectedRows.Count);
            _target.SelectedRows.Remove("kalle");
            Assert.AreEqual(0, _target.SelectedRows.Count);
        }
    }
}
