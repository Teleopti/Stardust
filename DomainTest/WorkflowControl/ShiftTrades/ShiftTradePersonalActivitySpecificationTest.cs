using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradePersonalActivitySpecificationTest
	{
		private ShiftTradePersonalActivitySpecification _target;
		private MockRepository _mocks;
		private IList<IShiftTradeSwapDetail> _details;
		private IShiftTradeSwapDetail _shiftTradeSwapDetail;
		private IPerson _personFrom;
		private IPerson _personTo;
		private IScheduleDay _scheduleDayFrom;
		private IScheduleDay _scheduleDayTo;
		private IPersonalShift _personalShift;
		private IPersonAssignment _personAssignmentFrom;
		private IPersonAssignment _personAssignmentTo;
		private IEditableShift _mainShift;
		private ILayerCollection<IActivity> _layerCollectionFrom;
		private ILayerCollection<IActivity> _layerCollectionTo;
		private DateTimePeriod? _periodFrom;
		private DateTimePeriod? _periodTo;
		private ReadOnlyCollection<IPersonalShift> _personalShifts;

		[SetUp]
		public void Setup()
		{
			_target = new ShiftTradePersonalActivitySpecification();
			_mocks = new MockRepository();
			_personFrom = _mocks.StrictMock<IPerson>();
			_personTo = _mocks.StrictMock<IPerson>();
			_scheduleDayFrom = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayTo = _mocks.StrictMock<IScheduleDay>();
			_personAssignmentFrom = _mocks.StrictMock<IPersonAssignment>();
			_personAssignmentTo = _mocks.StrictMock<IPersonAssignment>();
			_mainShift = _mocks.StrictMock<IEditableShift>();
			_layerCollectionFrom = _mocks.StrictMock<ILayerCollection<IActivity>>();
			_layerCollectionTo = _mocks.StrictMock<ILayerCollection<IActivity>>();
			_personalShift = _mocks.StrictMock<IPersonalShift>();
			_personalShifts = new ReadOnlyCollection<IPersonalShift>(new List<IPersonalShift> {_personalShift});
			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()) { SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo };
			_details = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail };
			var startFrom = new DateTime(2011,1,1,8,0,0,DateTimeKind.Utc);
			var endFrom = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			_periodFrom = new DateTimePeriod(startFrom, endFrom);
			
		}

		[Test]
		public void ShouldReturnDenyReason()
		{
			Assert.AreEqual("ShiftTradePersonalActivityDenyReason", _target.DenyReason);
		}

		[Test]
		public void ShouldReturnTrueWhenShiftCoverPersonalShift()
		{
			var startTo = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleDayFrom.AssignmentHighZOrder()).Return(_personAssignmentFrom);
				Expect.Call(_scheduleDayFrom.GetEditorShift()).Return(_mainShift);
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionFrom);
				Expect.Call(_layerCollectionFrom.Period()).Return(_periodFrom);

				Expect.Call(_scheduleDayTo.AssignmentHighZOrder()).Return(_personAssignmentTo);
				Expect.Call(_scheduleDayTo.GetEditorShift()).Return(null);
				Expect.Call(_personAssignmentTo.PersonalShiftCollection).Return(_personalShifts);
				Expect.Call(_personalShift.LayerCollection).Return(_layerCollectionTo);
				Expect.Call(_layerCollectionTo.Period()).Return(_periodTo);
			}

			using(_mocks.Playback())
			{
				Assert.IsTrue(_target.IsSatisfiedBy(_details));
			}
		}

		[Test]
		public void ShouldReturnFalseWhenShiftDoNotCoverPersonalShift()
		{
			var startTo = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 19, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			using(_mocks.Record())
			{
				Expect.Call(_scheduleDayTo.AssignmentHighZOrder()).Return(_personAssignmentTo);
				Expect.Call(_scheduleDayTo.GetEditorShift()).Return(_mainShift);
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionTo);
				Expect.Call(_layerCollectionTo.Period()).Return(_periodFrom);

				Expect.Call(_scheduleDayFrom.AssignmentHighZOrder()).Return(_personAssignmentFrom);
				Expect.Call(_scheduleDayFrom.GetEditorShift()).Return(null);
				Expect.Call(_personAssignmentFrom.PersonalShiftCollection).Return(_personalShifts);
				Expect.Call(_personalShift.LayerCollection).Return(_layerCollectionFrom);
				Expect.Call(_layerCollectionFrom.Period()).Return(_periodTo);
			}

			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));	
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			_target.IsSatisfiedBy(null);
		}
	}
}
