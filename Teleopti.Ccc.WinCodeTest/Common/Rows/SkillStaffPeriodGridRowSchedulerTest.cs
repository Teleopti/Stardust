using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Rows;


namespace Teleopti.Ccc.WinCodeTest.Common.Rows
{
    [TestFixture]
    public class SkillStaffPeriodGridRowSchedulerTest
    {
        private SkillStaffPeriodGridRowScheduler _target;
        private IRowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod> _rowManager;
        private MockRepository mocks;
        private DateTime _baseDate;
        private string _rowHeaderText;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _baseDate = new DateTime(2008, 10, 23, 0, 0, 0, DateTimeKind.Utc);
            _rowManager = mocks.StrictMock<IRowManager<SkillStaffPeriodGridRowScheduler, ISkillStaffPeriod>>();
            _rowHeaderText = "RowHeader";
			_target = new SkillStaffPeriodGridRowScheduler(_rowManager, "CellType", "Payload.ManualAgents", _rowHeaderText);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual("CellType", _target.CellType);
			Assert.AreEqual("Payload.ManualAgents", _target.DisplayMember);
            Assert.AreEqual(_rowHeaderText, _target.RowHeaderText);
        }

        [Test]
        public void VerifyOnQueryCellInfoWithoutData()
        {
            using (mocks.Record())
            {
                Expect.Call(_rowManager.DataSource).Return(new List<ISkillStaffPeriod>());
            }
            CellInfo cellInfo = new CellInfo {ColIndex = 1};
            using(mocks.Playback())
            {
                _target.QueryCellInfo(cellInfo);
            }
        }

        [Test]
        public void VerifyOnQueryCellInfoWithTooFewIntervals()
        {
            ISkillStaffPeriod skillStaffPeriod = mocks.StrictMock<ISkillStaffPeriod>();
            using (mocks.Record())
            {
                Expect.Call(_rowManager.DataSource).Return(new List<ISkillStaffPeriod>
                                                               {
                                                                   skillStaffPeriod
                                                               });
                Expect.Call(_rowManager.Intervals).Return(new List<IntervalDefinition>
                                                              {new IntervalDefinition(_baseDate, TimeSpan.Zero)})
                                                              .Repeat.Twice();
            }
            CellInfo cellInfo = new CellInfo
                                    {
                                        ColIndex = 3,
                                        RowHeaderCount = 1
                                    };
            using (mocks.Playback())
            {
                _target.QueryCellInfo(cellInfo);
            }
        }

        [Test]
        public void VerifyOnQueryCellInfoWithClosedDay()
        {
            ISkillStaffPeriod skillStaffPeriod1 = mocks.StrictMock<ISkillStaffPeriod>();
            using (mocks.Record())
            {
                Expect.Call(_rowManager.DataSource).Return(new List<ISkillStaffPeriod>
                                                               {
                                                                   skillStaffPeriod1
                                                               }).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.Intervals).Return(new List<IntervalDefinition>
                                                              {
                                                                  new IntervalDefinition(_baseDate, TimeSpan.Zero), 
                                                                  new IntervalDefinition(_baseDate.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15))
                                                              }).
                    Repeat.AtLeastOnce();
                Expect.Call(skillStaffPeriod1.Period).Return(
                    new DateTimePeriod(_baseDate, _baseDate).MovePeriod(TimeSpan.FromMinutes(30)));
            }
            CellInfo cellInfo = new CellInfo
            {
                ColIndex = 2,
                RowHeaderCount = 1,
                Style = new GridStyleInfo()
            };
            using (mocks.Playback())
            {
                _target.QueryCellInfo(cellInfo);
            }
            Assert.AreEqual(UserTexts.Resources.Closed,cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyOnQueryCellInfoWithNoColSpan()
        {
            var value = 2.4d;
            CellInfo cellInfo = new CellInfo
            {
                ColIndex = 2,
                RowHeaderCount = 1,
                Style = new GridStyleInfo()
            };

			ISkillStaffPeriod skillStaffPeriod1 = new SkillStaffPeriod(
					new DateTimePeriod(_baseDate, _baseDate).MovePeriod(TimeSpan.FromMinutes(15)).ChangeEndTime(
						TimeSpan.FromMinutes(15)), new Task(), new ServiceAgreement());
        	skillStaffPeriod1.Payload.ManualAgents = value;

            using (mocks.Record())
            {
                Expect.Call(_rowManager.DataSource).Return(new List<ISkillStaffPeriod>
                                                               {
                                                                   skillStaffPeriod1
                                                               }).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.Intervals).Return(new List<IntervalDefinition>
                                                              {
                                                                  new IntervalDefinition(_baseDate, TimeSpan.Zero), 
                                                                  new IntervalDefinition(_baseDate.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15))
                                                              }).
                    Repeat.AtLeastOnce();
                Expect.Call(_rowManager.IntervalLength).Return(15).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                _target.QueryCellInfo(cellInfo);
            }
            Assert.AreEqual(value, cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyOnQueryCellInfoWithColSpan()
        {
            var value = 2.4d;
            CellInfo cellInfo = new CellInfo
            {
                ColIndex = 1,
                RowHeaderCount = 1,
                Style = new GridStyleInfo()
            };
			
			ISkillStaffPeriod skillStaffPeriod1 = new SkillStaffPeriod(new DateTimePeriod(_baseDate, _baseDate).ChangeEndTime(
						TimeSpan.FromMinutes(30)), new Task(), new ServiceAgreement());
        	skillStaffPeriod1.Payload.ManualAgents = value;


            ITeleoptiGridControl teleoptiGridControl = mocks.StrictMock<ITeleoptiGridControl>();
            using (mocks.Record())
            {
                Expect.Call(_rowManager.DataSource).Return(new List<ISkillStaffPeriod>
                                                               {
                                                                   skillStaffPeriod1
                                                               }).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.Intervals).Return(new List<IntervalDefinition>
                                                              {
                                                                  new IntervalDefinition(_baseDate, TimeSpan.Zero), 
                                                                  new IntervalDefinition(_baseDate.Add(TimeSpan.FromMinutes(15)), TimeSpan.FromMinutes(15))
                                                              }).
                    Repeat.AtLeastOnce();
                Expect.Call(_rowManager.IntervalLength).Return(15).Repeat.AtLeastOnce();
                Expect.Call(_rowManager.Grid).Return(teleoptiGridControl).Repeat.AtLeastOnce();
                Expect.Call(teleoptiGridControl.ColCount).Return(4).Repeat.AtLeastOnce();
                teleoptiGridControl.AddCoveredRange(GridRangeInfo.Empty);
                LastCall.IgnoreArguments().Repeat.Once();
            }
            using (mocks.Playback())
            {
                _target.QueryCellInfo(cellInfo);
            }
            Assert.AreEqual(value, cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyHeaderText()
        {
            CellInfo cellInfo = new CellInfo {ColIndex = 0,Style = new GridStyleInfo()};
            _target.QueryCellInfo(cellInfo);
            Assert.AreEqual(_rowHeaderText,cellInfo.Style.CellValue);
        }

        [Test]
        public void VerifyMethodsFromInterface()
        {
            _target.OnSelectionChanged(null, 0);
            Assert.IsTrue(true);
        }
    }
}