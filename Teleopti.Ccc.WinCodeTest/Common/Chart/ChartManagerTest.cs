using System.Drawing;
using NUnit.Framework;
using Syncfusion.Windows.Forms.Chart;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Chart;

namespace Teleopti.Ccc.WinCodeTest.Common.Chart
{
	[TestFixture]
	public class ChartManagerTest
	{
		private ChartManager _target;

		[SetUp]
		public void Setup()
		{
			_target = new ChartManager();
		}

		[Test]
		public void ShouldHandleSeriesIndexLowerThanZero()
		{
			_target.SetChartToolTip(new ChartRegion(new Region(), -1, 0, "", ""), new ChartControl());
		}

		[Test]
		public void ShouldHandleSeriesIndexHigherThanMaxIndex()
		{
			_target.SetChartToolTip(new ChartRegion(new Region(), 1, 0, "", ""), new ChartControl());
		}
	}
}