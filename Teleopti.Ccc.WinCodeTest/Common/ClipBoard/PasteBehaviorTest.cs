using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
    [TestFixture]
    public class PasteBehaviorTest
    {
        private PasteBehavior _targetStandard;
        private PasteBehaviorForTest _testPasteBehavior;
        private MockRepository mockRep = new MockRepository();

        [SetUp]
        public void Setup()
        {
            _targetStandard = new NormalPasteBehavior();
            _testPasteBehavior = new PasteBehaviorForTest();
           
        }


        [Test]
        public void VerifyCanCreateDefaultPasteBehavior()
        {
            Assert.IsNotNull(_targetStandard);
        
        }

        [Test]
        public void VerifyCanOnlyPasteWithinGrid()
        {
            using (GridControl gridControl = new GridControl())
            {
                gridControl.SetRowHeight(0, 10, 5);
                gridControl.SetColWidth(0, 10, 5);

                Assert.IsTrue(_testPasteBehavior.FitsInsideGrid(gridControl, 1, 1, 9, 9));
                Assert.IsFalse(_testPasteBehavior.FitsInsideGrid(gridControl, 2, 1, 9, 9), "Outside RowEnd");
                Assert.IsFalse(_testPasteBehavior.FitsInsideGrid(gridControl, 1, 2, 9, 9), "Outside ColumndEnd");
                Assert.IsFalse(_testPasteBehavior.FitsInsideGrid(gridControl, 1, -8, 1, 1), "Outside ColumnStart");
                Assert.IsFalse(_testPasteBehavior.FitsInsideGrid(gridControl, -2, 1, 1, 1), "Outside RowStart");
            }
        }

        [Test]
        public void VerifyCanOnlyPasteWithinRange()
        {
            GridRangeInfo range = GridRangeInfo.Cells(10, 5, 5, 10); //Top,Left,Bottom,Right
            Assert.IsTrue(_testPasteBehavior.FitsInsideRange(range, 0, 0, 5, 5));
            Assert.IsFalse(_testPasteBehavior.FitsInsideRange(range, 0, 6, 5, 5), "Column is outside Right");
            Assert.IsFalse(_testPasteBehavior.FitsInsideRange(range, 0, 0, 5, 3), "Column is outside Left");
            Assert.IsFalse(_testPasteBehavior.FitsInsideRange(range, 6, 2, 5, 5), "Row is outside Top");
            Assert.IsFalse(_testPasteBehavior.FitsInsideRange(range, 2, 2, 2, 5), "Row is outside Bottom");
            GridRangeInfo emptyRange = GridRangeInfo.Cells(10, 10, 10, 10);
            Assert.IsTrue(_testPasteBehavior.FitsInsideRange(emptyRange, 2, 2, 5, 15), "Ok if range is Empty even if outside");
        }

        [Test]
        public void VerifyIsPasteRangeOkFails()
        {
            GridRangeInfo range = GridRangeInfo.Cells(15, 5, 5, 10);
            using (GridControl gridControl = new GridControl())
            {
                gridControl.SetRowHeight(0, 10, 5);
                gridControl.SetColWidth(0, 10, 5);

                Assert.IsTrue(_testPasteBehavior.FitsInsideRange(range, 0, 0, 14, 5));
                Assert.IsTrue(_testPasteBehavior.FitsInsideGrid(gridControl, 0, 0, 4, 5));
                Assert.IsFalse(_testPasteBehavior.IsPasteRangeOk(range, gridControl, 0, 0, 4, 5), "Range Fails");
                Assert.IsFalse(_testPasteBehavior.IsPasteRangeOk(range, gridControl, 0, 0, 14, 5), "Grid Fails");
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-09-04
        /// </remarks>
        [Test]
        public void VerifyCanDoStandardPaste()
        {
            //Disclaimer(henrika 08-09-04): test written for existing code
            //TODO: Rewrite code w. test first
            int col = 2;
            int row = 2;

            using(GridControl gridControl = new GridControl())
            {
                GridRangeInfo range = GridRangeInfo.Cells(row, 0, 0, col);
                gridControl.SetRowHeight(0, 100, 5);
                gridControl.SetColWidth(0, 100, 5);
                ClipHandler<string> handler = new ClipHandler<string>();

                PasteActionForTest<string> pasteActionForTest = new PasteActionForTest<string>();
                GridRangeInfoList rangeList = new GridRangeInfoList();
                rangeList.Add(range);

                _testPasteBehavior.DoPaste<string>(gridControl, handler, pasteActionForTest, rangeList);
                Assert.AreEqual(0, pasteActionForTest.CalledTimes, "Not called when Cliplist is empty");
                handler.AddClip(1, 1, "clip for test");
                _testPasteBehavior.DoPaste<string>(gridControl, handler, pasteActionForTest, rangeList);
                Assert.AreEqual((row*col), pasteActionForTest.CalledTimes);

                IList<string> list = _testPasteBehavior.DoPaste<string>(gridControl, handler, pasteActionForTest,
                                                                        rangeList);
                Assert.AreEqual(0, list.Count);

                pasteActionForTest.SetRetValue("clip for test");
                list = _testPasteBehavior.DoPaste<string>(gridControl, handler, pasteActionForTest, rangeList);
                Assert.AreEqual(4, list.Count);
            }
        }

       

        [Test]
        public void VerifyNormalPaste()
        {
            NormalPasteBehavior normalBehavior = new NormalPasteBehavior();

            IScheduleDay part = mockRep.StrictMock<IScheduleDay>();
	        var personAssignment = mockRep.StrictMock<IPersonAssignment>();

            using (GridControl gridControl = new GridControl())
            {
                GridRangeInfo range = GridRangeInfo.Cells(1, 1, 1, 2);
                gridControl.SetRowHeight(0, 100, 5);
                gridControl.SetColWidth(0, 100, 5);
                ClipHandler<IScheduleDay> handler = new ClipHandler<IScheduleDay>();

                GridRangeInfoList rangeList = new GridRangeInfoList();
                rangeList.Add(range);
                IGridPasteAction<IScheduleDay> pasteAction = mockRep.StrictMock<IGridPasteAction<IScheduleDay>>();
                handler.AddClip(0, 0, part);
                Clip<IScheduleDay> clip = handler.ClipList[0];

                using (mockRep.Record())
                {
                    Expect.Call(part.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.AtLeastOnce();
                    Expect.Call(part.PersonAbsenceCollection()).Return(new IPersonAbsence[0]).Repeat.AtLeastOnce();
                    Expect.Call(pasteAction.PasteBehavior).Return(normalBehavior);
                    Expect.Call(pasteAction.Paste(gridControl, clip, 1, 1)).Return(null);
                    Expect.Call(pasteAction.Paste(gridControl, clip, 1, 2)).Return(part);
	                Expect.Call(part.PersonAssignment()).Return(personAssignment).Repeat.AtLeastOnce();
	                Expect.Call(() => part.Remove(personAssignment)).Repeat.AtLeastOnce();
	                Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2001, 1, 1), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));
	                Expect.Call(personAssignment.Period).Return(new DateTimePeriod(2013, 1, 1, 2013, 1, 1)).Repeat.AtLeastOnce();
	                Expect.Call(pasteAction.PasteOptions).Return(new PasteOptions()).Repeat.AtLeastOnce();
                }

                using (mockRep.Playback())
                {
                    pasteAction.PasteBehavior.DoPaste(gridControl, handler, pasteAction, rangeList);
                }
            }
        }

        [Test]
        public void VerifyExtendedPaste()
        {
            ExtenderPasteBehavior extendedBehavior = new ExtenderPasteBehavior();

            using (GridControl gridControl = new GridControl())
            {
                GridRangeInfo range = GridRangeInfo.Cells(1, 1, 1, 1);
                gridControl.SetRowHeight(0, 100, 5);
                gridControl.SetColWidth(0, 100, 5);
                ClipHandler<string> handler = new ClipHandler<string>();
                handler.AddClip(1, 1, "MamaiRaja");

                GridRangeInfoList rangeList = new GridRangeInfoList();
                rangeList.Add(range);
                IGridPasteAction<string> pasteAction = mockRep.StrictMock<IGridPasteAction<string>>();

                Clip<string> clip = handler.ClipList[0];

                using (mockRep.Record())
                {
                    Expect
                        .On(pasteAction)
                        .Call(pasteAction.PasteBehavior)
                        .Return(extendedBehavior)
                        .Repeat.Any();

                    Expect
                        .On(pasteAction)
                        .Call(pasteAction.Paste(gridControl, clip, 1, 1))
                        .Return(clip.ClipValue)
                        .Repeat.Any();
                }

                using (mockRep.Playback())
                {
                    pasteAction.PasteBehavior.DoPaste(gridControl, handler, pasteAction, rangeList);
                }
            }
        }

		[Test]
		public void ShouldNotAdjustFullDayAbsenceWhenShiftDontEndNextDay()
		{
			var destination = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var part = ScheduleDayFactory.Create(new DateOnly(2000, 1, 1));
			var period8To16 = new DateTimePeriod(new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 16, 0, 0, DateTimeKind.Utc));
			var periodFullDay = new DateTimePeriod(2000,1,1,2000,1,2);
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(destination.Person, destination.Scenario, periodFullDay);
			var personAbsenceFull = PersonAbsenceFactory.CreatePersonAbsence(part.Person, part.Scenario, periodFullDay);
			var shiftOnDay = EditableShiftFactory.CreateEditorShift(new Activity("activity"), period8To16, new ShiftCategory("shiftCategory"));
			var shift = EditableShiftFactory.CreateEditorShift(new Activity("activity"), period8To16, new ShiftCategory("shiftCategory"));
			
			part.AddMainShift(shift);
			part.Add(personAbsenceFull);
			destination.AddMainShift(shiftOnDay);
			destination.Add(personAbsence);

			using (var gridControl = new GridControl())
			{
				var range = GridRangeInfo.Cells(1, 1, 1, 1);
				gridControl.SetRowHeight(0, 100, 5);
				gridControl.SetColWidth(0, 100, 5);
				var handler = new ClipHandler<IScheduleDay>();

				var rangeList = new GridRangeInfoList();
				rangeList.Add(range);
				var pasteAction = mockRep.StrictMock<IGridPasteAction<IScheduleDay>>();
				handler.AddClip(0, 0, part);
				var clip = handler.ClipList[0];

				using (mockRep.Record())
				{
					Expect.Call(pasteAction.PasteBehavior).Return(new NormalPasteBehavior());
					Expect.Call(pasteAction.Paste(gridControl, clip, 1, 1)).Return(destination);
					Expect.Call(pasteAction.PasteOptions).Return(new PasteOptions()).Repeat.AtLeastOnce();
				}

				using (mockRep.Playback())
				{
					pasteAction.PasteBehavior.DoPaste(gridControl, handler, pasteAction, rangeList);
					Assert.AreEqual(destination.PersonAbsenceCollection()[0].Period, periodFullDay);
				}
			}
		}
    }


    internal class PasteActionForTest<T> : IGridPasteAction<T>
    {
        private T _retValue;// = true;
        private int _calledTimes;

        internal void SetRetValue(T value)
        {
            _retValue = value;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        internal int CalledTimes
        {
            get { return _calledTimes; }
        }

        public T Paste(GridControl gridControl,  Clip<T> clip, int rowIndex, int columnIndex)
        {
            _calledTimes++;
            return _retValue;
        }

        public IPasteBehavior PasteBehavior
        {
            get { return new PasteBehaviorForTest(); }
        }

		public PasteOptions PasteOptions
		{
			get { return new PasteOptions(); }
		}

    }

    internal sealed class PasteBehaviorForTest : NormalPasteBehavior
    {
      
        internal new bool IsPasteRangeOk(GridRangeInfo range, GridControl grid, int clipRowOffset, int clipColOffset, int row, int col)
        {
            return base.IsPasteRangeOk(range, grid, clipRowOffset, clipColOffset, row, col);
        }

        internal new bool FitsInsideGrid(GridControl grid, int clipRowOffset, int clipColOffset, int row, int col)
        {
            return base.FitsInsideGrid(grid, clipRowOffset, clipColOffset, row, col);
        }

        internal new bool FitsInsideRange(GridRangeInfo range, int clipRowOffset, int clipColOffset, int row, int col)
        {
            return base.FitsInsideRange(range, clipRowOffset, clipColOffset, row, col);
        }
    }
}