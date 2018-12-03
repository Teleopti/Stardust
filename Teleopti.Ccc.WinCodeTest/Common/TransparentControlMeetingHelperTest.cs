using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;


namespace Teleopti.Ccc.WinCodeTest.Common
{
	[TestFixture]
	public class TransparentControlMeetingHelperTest
	{
		private TransparentControlMeetingHelper _transparentControlHelper;
		
		[SetUp]
		public void Setup()
		{
			var minMaxBorders = new MinMax<int>(10, 20);
			var minMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			var minMaxTimePos = new MinMax<int>(10, 20);
			
			_transparentControlHelper = new TransparentControlMeetingHelper(minMaxBorders, minMaxTime, minMaxTimePos, 15);
		}
		
		[Test]
		public void ShouldInitialize()
		{

			var minMaxBorders = new MinMax<int>(10, 20);
			var minMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			var minMaxTimePos = new MinMax<int>(10, 20);

			_transparentControlHelper = new TransparentControlMeetingHelper(minMaxBorders, minMaxTime, minMaxTimePos, 15);

			Assert.AreEqual(minMaxBorders, _transparentControlHelper.MinMaxBorders);
			Assert.AreEqual(minMaxTime, _transparentControlHelper.MinMaxTime);
			Assert.AreEqual(minMaxTimePos, _transparentControlHelper.MinMaxTimePos);
			Assert.AreEqual(15, _transparentControlHelper.SnapTo);
		}
		
		[Test]
		public void ShouldGetMinLeftWhenLeftPositionLessThanMinLeft()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(10, 20);
			Assert.AreEqual(10, _transparentControlHelper.GetLeftPosition(5, 10));
		}

		[Test] 
		public void ShouldGetRightPositionMinusMinWidthWhenLeftBorderGreaterThanRightPosition()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(0, 900);
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(0, 900);
			Assert.AreEqual(25, _transparentControlHelper.GetLeftPosition(51, 50));
		}
	
		[Test]
		public void ShouldGetPositionWhenPositionBetweenMinLeftAndRightBorder()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(10, 50);
			Assert.AreEqual(20, _transparentControlHelper.GetLeftPosition(20, 40));
		}

		[Test]
		public void ShouldGetMaxRightWhenRightPositionGreaterThanMaxRight()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(10, 20);
			Assert.AreEqual(20, _transparentControlHelper.GetRightPosition(15, 40));
		}

		[Test]
		public void ShouldGetLeftPositionPlusMinWidthWhenRightBorderPosLessThenLeftPosition()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(0, 900);
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(0, 900);

			Assert.AreEqual(75, _transparentControlHelper.GetRightPosition(50, 49));
		}

		[Test]
		public void ShouldGetPositionWhenRightBorderBetweenLeftAndMaxRightPosition()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(5, 50);
			Assert.AreEqual(40, _transparentControlHelper.GetRightPosition(10, 40));
		}

		[Test]
		public void ShouldGetMinLeftWhenPositionLessThanMinLeftWhenConsiderWidth()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(50, 100);
			Assert.AreEqual(50, _transparentControlHelper.GetLeftPositionConsiderWidth(20, 20));
		}

		[Test]
		public void ShouldGetMaxRightMinusWidthWhenPositionToGreatWhenConsiderWidth()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(50, 100);
			Assert.AreEqual(70, _transparentControlHelper.GetLeftPositionConsiderWidth(80, 30));
		}

		[Test]
		public void ShouldGetLeftPositionWhenLeftPlusWidthFits()
		{
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(10, 100);
			Assert.AreEqual(30, _transparentControlHelper.GetLeftPositionConsiderWidth(30, 40));
		}

		[Test]
		public void ShouldGetPositionFromTimeSpan()
		{
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));

			Assert.AreEqual(100, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8)));
			Assert.AreEqual(200, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(9)));
			Assert.AreEqual(1000, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(17)));

			Assert.AreEqual(150, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))));
			Assert.AreEqual(125, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15))));

			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 550);
			Assert.AreEqual(100, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8)));
			Assert.AreEqual(150, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(9)));
			Assert.AreEqual(550, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(17)));

			Assert.AreEqual(125, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))));
			Assert.AreEqual(112, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15))));
		}

		[Test]
		public void ShouldGetColumnFromTimeSpan()
		{
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
	
			Assert.AreEqual(2, _transparentControlHelper.GetColumnFromTimeSpan(TimeSpan.FromHours(9), 0, 60));
		}

		[Test]
		public void ShouldGetColumnFromTimeSpanWhenRightToLeft()
		{
			_transparentControlHelper.IsRightToLeft = true;
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			
			Assert.AreEqual(2, _transparentControlHelper.GetColumnFromTimeSpan(TimeSpan.FromHours(9), 0, 60));
		}

		[Test]
		public void ShouldGetPositionFromTimeSpanWhenRightToLeft()
		{
			_transparentControlHelper.IsRightToLeft = true;
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));

			Assert.AreEqual(1000, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8)));
			Assert.AreEqual(900, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(9)));
			Assert.AreEqual(100, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(17)));

			Assert.AreEqual(950, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))));
			Assert.AreEqual(975, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15))));

			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 550);
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(100, 550);
			Assert.AreEqual(550, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8)));
			Assert.AreEqual(500, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(9)));
			Assert.AreEqual(100, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(17)));

			Assert.AreEqual(525, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30))));
			Assert.AreEqual(538, _transparentControlHelper.GetPositionFromTimeSpan(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15))));
		}

		[Test]
		public void ShouldGetTimeSpanFromPosition()
		{
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);

			Assert.AreEqual(TimeSpan.FromHours(8), _transparentControlHelper.GetTimeSpanFromPosition(100));
			Assert.AreEqual(TimeSpan.FromHours(9), _transparentControlHelper.GetTimeSpanFromPosition(200));
			Assert.AreEqual(TimeSpan.FromHours(17), _transparentControlHelper.GetTimeSpanFromPosition(1000));

			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)), _transparentControlHelper.GetTimeSpanFromPosition(150));
			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), _transparentControlHelper.GetTimeSpanFromPosition(125));

			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 550);

			Assert.AreEqual(TimeSpan.FromHours(8), _transparentControlHelper.GetTimeSpanFromPosition(100));
			Assert.AreEqual(TimeSpan.FromHours(9), _transparentControlHelper.GetTimeSpanFromPosition(150));
			Assert.AreEqual(TimeSpan.FromHours(17), _transparentControlHelper.GetTimeSpanFromPosition(550));

			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)), _transparentControlHelper.GetTimeSpanFromPosition(125));
			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), _transparentControlHelper.GetTimeSpanFromPosition(112));
		}

		[Test]
		public void ShouldGetTimeSpanFromPositionWhenRightToLeft()
		{
			_transparentControlHelper.IsRightToLeft = true;
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(100, 1000);

			Assert.AreEqual(TimeSpan.FromHours(8), _transparentControlHelper.GetTimeSpanFromPosition(1000));
			Assert.AreEqual(TimeSpan.FromHours(9), _transparentControlHelper.GetTimeSpanFromPosition(900));
			Assert.AreEqual(TimeSpan.FromHours(17), _transparentControlHelper.GetTimeSpanFromPosition(100));

			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)), _transparentControlHelper.GetTimeSpanFromPosition(950));
			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), _transparentControlHelper.GetTimeSpanFromPosition(975));

			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 550);
			_transparentControlHelper.MinMaxBorders = new MinMax<int>(100, 550);

			Assert.AreEqual(TimeSpan.FromHours(8), _transparentControlHelper.GetTimeSpanFromPosition(550));
			Assert.AreEqual(TimeSpan.FromHours(9), _transparentControlHelper.GetTimeSpanFromPosition(500));
			Assert.AreEqual(TimeSpan.FromHours(17), _transparentControlHelper.GetTimeSpanFromPosition(100));

			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30)), _transparentControlHelper.GetTimeSpanFromPosition(525));
			Assert.AreEqual(TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(15)), _transparentControlHelper.GetTimeSpanFromPosition(538));
		}

		[Test]
		public void ShouldSnapTo()
		{
			_transparentControlHelper.MinMaxTime = new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(17));
			_transparentControlHelper.MinMaxTimePos = new MinMax<int>(100, 1000);
			
			_transparentControlHelper.SnapTo = 60;

			Assert.AreEqual(200, _transparentControlHelper.GetSnappedPosition(190));
			Assert.AreEqual(100, _transparentControlHelper.GetSnappedPosition(140));

			_transparentControlHelper.SnapTo = 30;

			Assert.AreEqual(150, _transparentControlHelper.GetSnappedPosition(140));
			Assert.AreEqual(100, _transparentControlHelper.GetSnappedPosition(120));

			_transparentControlHelper.SnapTo = 15;

			Assert.AreEqual(125, _transparentControlHelper.GetSnappedPosition(120));
			Assert.AreEqual(100, _transparentControlHelper.GetSnappedPosition(110));

			_transparentControlHelper.SnapTo = 5;

			Assert.AreEqual(133, _transparentControlHelper.GetSnappedPosition(135));
			Assert.AreEqual(100, _transparentControlHelper.GetSnappedPosition(101));
		}
	}
}
