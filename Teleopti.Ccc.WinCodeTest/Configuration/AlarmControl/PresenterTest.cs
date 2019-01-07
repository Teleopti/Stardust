using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.WinCodeTest.Configuration.AlarmControl
{
	[TestFixture, SetUICulture("en-US")]
    public class AlarmControlPresenterTest
    {
        private AlarmControlPresenter _target;
        private MockRepository mocks;
        private IList<IRtaRule> _rules;
        private IAlarmControlView _view;
        private IPerson _createPerson;
        private DateTime _createDate;

        [SetUp]
        public void SetupAlarmControlPresenter()
        {
            mocks = new MockRepository();

            _rules = new List<IRtaRule>();
            _rules.Add(new RtaRule(new Description("userALARM"), Color.Blue, 1, 0.8));
            _rules.Add(new RtaRule(new Description("moo"), Color.Blue, 1, 0.8));
            _rules.Add(new RtaRule(new Description("ok"), Color.Blue, 1, 0.8));
            _rules.Add(new RtaRule(new Description("unknown"), Color.Blue, 1, 0.8));
            SetCreatedInfo(_rules);
            _view = mocks.StrictMock<IAlarmControlView>();
            _target = new AlarmControlPresenter(_rules, _view, null);
        }

        private void SetCreatedInfo(IEnumerable<IRtaRule> alarms)
        {
            _createPerson = PersonFactory.CreatePerson();
            _createDate = new DateTime(2009, 1, 1);
            _createPerson.PermissionInformation.SetDefaultTimeZone((
                                                                       TimeZoneInfo.FindSystemTimeZoneById(
                                                                           "W. Europe Standard Time")));
        }

        [Test]
        public void QueryRowCountShouldReturnAlarmTypeCount()
        {
            var e = new GridRowColCountEventArgs();
            _target.QueryRowCount(null, e);
            Assert.AreEqual(_rules.Count, e.Count);
        }

        [Test]
        public void QueryColumnCountShouldReturnColumnTypeCountMinusOne()
        {
            var e = new GridRowColCountEventArgs();
            _target.QueryColCount(null, e);
			Assert.AreEqual(_target.Columns.Count(), e.Count);
        }

        [Test]
        public void QueryCellInfoWithBadIndex()
        {
            _target = new AlarmControlPresenter(new List<IRtaRule>(), _view, null);
            var style = new GridStyleInfo();
            var e = new GridQueryCellInfoEventArgs(-1, -1, style);
            _target.QueryCellInfo(null, e);
        }

        [Test]
        public void QueryHeaderShouldReturnColumnHeader()
        {
            _target = new AlarmControlPresenter(new List<IRtaRule>(), _view, null);
            var style = new GridStyleInfo();
            var e = new GridQueryCellInfoEventArgs(0, (int)AlarmControlPresenter.ColumnHeader.Name, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(Resources.Name, e.Style.Text);

            e = new GridQueryCellInfoEventArgs(0, (int)AlarmControlPresenter.ColumnHeader.StaffingEffect, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(Resources.StaffingEffect, e.Style.Text);

            e = new GridQueryCellInfoEventArgs(0, (int)AlarmControlPresenter.ColumnHeader.Color, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(Resources.Color, e.Style.Text);

            e = new GridQueryCellInfoEventArgs(0, (int)AlarmControlPresenter.ColumnHeader.Time, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(Resources.Time, e.Style.Text);
        }

        [Test]
        public void QueryCellInfoShouldReturnGrid()
        {
 
            var style = new GridStyleInfo();
            var e = new GridQueryCellInfoEventArgs(0, (int)AlarmControlPresenter.ColumnHeader.Name, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("Name", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(1, 0, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(string.Empty, style.Text); //the numberformatting is a secret?

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(3, (int)AlarmControlPresenter.ColumnHeader.Name, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("ok", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Time, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("1", style.Text);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(4, (int)AlarmControlPresenter.ColumnHeader.Color, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(Color.Blue, style.CellValue);

            style = new GridStyleInfo();
            e = new GridQueryCellInfoEventArgs(4, (int)AlarmControlPresenter.ColumnHeader.StaffingEffect, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual(0.8, style.CellValue);

            style = new GridStyleInfo();

            e = new GridQueryCellInfoEventArgs(0, 0, style);
            _target.QueryCellInfo(null, e);
            Assert.AreEqual("", style.Text); //todo test that this cell is empty... or?
        }

        [Test]
        public void CellClickShouldReturnIndexOfSelectedAlarm()
        {
            using (mocks.Record())
            {
                _view.ShowThisItem(2);
            }
            mocks.ReplayAll();
            var e = new GridCellClickEventArgs(3, 4, null, false);
            _target.CellClick(null, e);
        }

        [Test]
        public void SaveCellInfoNameChangeShouldUpdateDescriptionName()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Name, style, StyleModifyType.Changes);
            e.Style.Text = "foo";
            _target.SaveCellInfo(null, e);
            Assert.AreEqual("foo", _rules[0].Description.Name);
        }

        [Test]
        public void SaveCellInfoTimeShouldChangeThresholdTime()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Time, style, StyleModifyType.Changes);
            e.Style.CellValue = 45;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(45.0d, _rules[0].ThresholdTime);
        }

        [Test]
        public void SaveCellInfoStaffingEffectShouldChangeStaffingEffect()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.StaffingEffect, style, StyleModifyType.Changes);
            e.Style.CellValue = 45.0d;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(45.0d, _rules[0].StaffingEffect);
        }

        [Test]
        public void SaveCellInfoColorShouldChangeColor()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Color, style, StyleModifyType.Changes);
            e.Style.CellValue = Color.Red;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(Color.Red, _rules[0].DisplayColor);
        }

        [Test]
        public void SaveCellInfoDeleteShouldSetSelectedCellsToDefaultValueOrNotChangeIfDefaultValueIsIllegal()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Name, style, StyleModifyType.Changes);
            _target.SaveCellInfo(null, e);
            Assert.AreEqual("userALARM", _rules[0].Description.Name);

            style = new GridStyleInfo();
            e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Time, style, StyleModifyType.Changes);
            e.Style.CellValue = 0;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(0.0d, _rules[0].ThresholdTime);

            style = new GridStyleInfo();
            e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.StaffingEffect, style, StyleModifyType.Changes);
            e.Style.CellValue = 0.0d;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(0.0d, _rules[0].StaffingEffect);

            style = new GridStyleInfo();
            e = new GridSaveCellInfoEventArgs(1, (int)AlarmControlPresenter.ColumnHeader.Color, style, StyleModifyType.Changes);
            e.Style.CellValue = Color.Empty;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(Color.Blue, _rules[0].DisplayColor);
        }

        [Test]
        public void SaveCellInfoShouldNotDeleteRowAndColumns()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(0, 1, style, StyleModifyType.Changes);
            _target.SaveCellInfo(null, e);

            style = new GridStyleInfo();
            e = new GridSaveCellInfoEventArgs(1, 0, style, StyleModifyType.Changes);
            _target.SaveCellInfo(null, e);
        }

        [Test]
        public void SaveCellInfoTimeSpanShouldNotBeNegative()
        {
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, 2, style, StyleModifyType.Changes);
            e.Style.CellValue = -10.0d;
            _target.SaveCellInfo(null, e);
            Assert.AreEqual(1.0d, _rules[0].ThresholdTime);
        }

        [Test]
        public void SaveCellInfoNameCannotBeDuplicate()
        {
            using (mocks.Record())
            {
                _view.Warning(Resources.NameAlreadyExists);
            }
            var style = new GridStyleInfo();
            var e = new GridSaveCellInfoEventArgs(1, 1, style, StyleModifyType.Changes);
            e.Style.CellValue = "moo";
            mocks.ReplayAll();
            _target.SaveCellInfo(null, e);
            Assert.AreEqual("userALARM", _rules[0].Description.Name);
        }
    }
}
