using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Common.Chart;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Chart
{
    [TestFixture]
    public class ChartSeriesSettingTest
    {
        private ChartSeriesSetting _target;

        [SetUp]
        public void Setup()
        {
            _target = new ChartSeriesSetting("Column.Name", Color.DimGray, ChartSeriesDisplayType.Line, true, AxisLocation.Right);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(),true));
        }

        [Test]
        public void ConstructorWorks()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void CanSetAndGetProperties()
        {
            
            _target.Color = Color.DodgerBlue;
            _target.SeriesType = ChartSeriesDisplayType.Bar;
            _target.Enabled = true;
            _target.AxisLocation = AxisLocation.Right;

            Assert.AreEqual(Color.DodgerBlue,_target.Color);
            Assert.AreEqual(ChartSeriesDisplayType.Bar, _target.SeriesType);
            Assert.AreEqual(true, _target.Enabled);
            Assert.AreEqual(AxisLocation.Right, _target.AxisLocation);
        }

        [Test]
        public void CanClone()
        {
            
            IChartSeriesSetting settingClone = (IChartSeriesSetting)_target.Clone();
            //Assert.IsFalse(settingClone.Id.HasValue);
            Assert.AreEqual(_target.AxisLocation, settingClone.AxisLocation);
            Assert.AreEqual(_target.Color, settingClone.Color);
            Assert.AreEqual(_target.DisplayKey, settingClone.DisplayKey);
            Assert.AreEqual(_target.Enabled, settingClone.Enabled);
            Assert.AreEqual(_target.SeriesType, settingClone.SeriesType);

            settingClone = _target.NoneEntityClone();
            //Assert.IsFalse(settingClone.Id.HasValue);
            Assert.AreEqual(_target.AxisLocation, settingClone.AxisLocation);
            Assert.AreEqual(_target.Color, settingClone.Color);
            Assert.AreEqual(_target.DisplayKey, settingClone.DisplayKey);
            Assert.AreEqual(_target.Enabled, settingClone.Enabled);
            Assert.AreEqual(_target.SeriesType, settingClone.SeriesType);

            settingClone = _target.EntityClone();
            //Assert.AreEqual(_target.Id.Value, settingClone.Id.Value);
            Assert.AreEqual(_target.AxisLocation, settingClone.AxisLocation);
            Assert.AreEqual(_target.Color, settingClone.Color);
            Assert.AreEqual(_target.DisplayKey, settingClone.DisplayKey);
            Assert.AreEqual(_target.Enabled, settingClone.Enabled);
            Assert.AreEqual(_target.SeriesType, settingClone.SeriesType);
        }
    }
}