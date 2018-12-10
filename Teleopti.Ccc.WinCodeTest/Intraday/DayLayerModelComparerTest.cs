using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class DayLayerModelComparerTest
	{
		private DayLayerModelComparer _target;
		private DayLayerModel x;
		private DayLayerModel y;


		[SetUp]
		public void Setup()
		{
			_target = new DayLayerModelComparer();
			var layerviewModelCollection = new LayerViewModelCollection(null, null, null, null, new FullPermission());
			var xTeam = new Team().WithDescription(new Description("xTeam"));
			var yTeam = new Team().WithDescription(new Description("yTeam"));
			x = new DayLayerModel(PersonFactory.CreatePerson("x", "x"), new DateTimePeriod(2000,01,01,2059,01,01), xTeam, layerviewModelCollection, new CommonNameDescriptionSetting());
			y = new DayLayerModel(PersonFactory.CreatePerson("y", "y"), new DateTimePeriod(2000, 01, 01, 2059, 01, 01), yTeam, layerviewModelCollection, new CommonNameDescriptionSetting());
		}

		[Test]
		public void ShouldCompare_TeamNames()
		{
			setSortDescription("Team.Description.Name");
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_PersonName()
		{
			setSortDescription("CommonNameDescription");
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_CurrentStateDescription()
		{
			setSortDescription("CurrentStateDescription");
			x.CurrentStateDescription = "X";
			y.CurrentStateDescription = "Y";
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_CurrentActivityDescription()
		{
			setSortDescription("CurrentActivityDescription");
			x.CurrentActivityDescription = "X";
			y.CurrentActivityDescription = "Y";
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_NextActivityDescription()
		{
			setSortDescription("NextActivityDescription");
			x.NextActivityDescription = "x";
			y.NextActivityDescription = "y";
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_NextActivityStartDateTime()
		{
			setSortDescription("NextActivityStartDateTime");
			x.NextActivityStartDateTime = new DateTime(2000,01,01);
			y.NextActivityStartDateTime = new DateTime(2013,06,27);
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_AlarmDescription()
		{
			setSortDescription("AlarmDescription");
			x.AlarmDescription = "x";
			y.AlarmDescription = "y";
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_EnteredCurrentState()
		{
			setSortDescription("EnteredCurrentState");
			x.EnteredCurrentState = new DateTime(2000,01,01);
			y.EnteredCurrentState = new DateTime(2013,06,27);
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_IsPinned()
		{
			setSortDescription("IsPinned");
			x.IsPinned = false;
			y.IsPinned = true;
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_ScheduleStartDateTime()
		{
			setSortDescription("ScheduleStartDateTime");
			x.ScheduleStartDateTime = new DateTime(2000,01,01);
			y.ScheduleStartDateTime = new DateTime(2013,06,27);
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldCompare_NextActivityStartDateTime_Descending()
		{
			_target.SortDescriptions = new List<SortDescription>
				{
					new SortDescription("NextActivityStartDateTime", ListSortDirection.Descending)
				};
			x.NextActivityStartDateTime = new DateTime(2000, 01, 01);
			y.NextActivityStartDateTime = new DateTime(2013, 06, 27);
			_target.Compare(x, y).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_NullOrEmptyStringLast_Ascending()
		{
			setSortDescription("CurrentStateDescription");
			x.CurrentStateDescription = null;
			y.CurrentStateDescription = "y";
			_target.Compare(x, y).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCompare_NullOrEmptyStringFirst_Descending()
		{
			_target.SortDescriptions = new List<SortDescription>
				{
					new SortDescription("CurrentStateDescription", ListSortDirection.Descending)
				};
			x.CurrentStateDescription = null;
			y.CurrentStateDescription = "y";
			_target.Compare(x, y).Should().Be.EqualTo(-1);
		}

		private void setSortDescription(string propetyName)
		{
			_target.SortDescriptions = new List<SortDescription>
				{
					new SortDescription(propetyName, ListSortDirection.Ascending)
				};
		}
	}
}
