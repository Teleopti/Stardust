using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeEmptyScheduleSpecificationTest
	{
		private ShiftTradeEmptyScheduleSpecification _target;
		private IList<IShiftTradeSwapDetail> _details;
		private IShiftTradeSwapDetail _shiftTradeSwapDetail;
		private IPerson _personFrom;
		private IPerson _personTo;
		private IScheduleDay _scheduleDayFrom;
		private IScheduleDay _scheduleDayTo;
		private IPersonAssignment _personAssignmentTo;
		private IDayOff _dayOff;
		private IProjectionService _projectionServiceTo;
		private IVisualLayer _visualLayerTo;
		private IVisualLayerCollection _visualLayerCollectionTo;

		[SetUp]
		public void Setup()
		{
			_target = new ShiftTradeEmptyScheduleSpecification();
			_personFrom = MockRepository.GenerateMock<IPerson>();
			_personTo = MockRepository.GenerateMock<IPerson>();

			_personAssignmentTo = MockRepository.GenerateMock<IPersonAssignment>();

			var startFrom = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endFrom = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			new DateTimePeriod(startFrom, endFrom);

		}

		[Test]
		public void ShouldReturnDenyReason()
		{
			Assert.AreEqual("ShiftTradeEmptyScheduleDenyReason", _target.DenyReason);
		}

		[Test]
		public void ShouldReturnFalseWhenHasNoSchedule()
		{
			_scheduleDayFrom = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDayTo = MockRepository.GenerateMock<IScheduleDay>();

			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()) { SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo };
			_details = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail };

			_target.IsSatisfiedBy(_details).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnTrueWhenHasShift()
		{
			_scheduleDayFrom = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDayTo = MockRepository.GenerateMock<IScheduleDay>();
			MockRepository.GenerateMock<IEditableShift>();
			new List<IEditableShiftLayer>();
			new List<IEditableShiftLayer>();
			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()) { SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo };
			_details = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail };

			_target.IsSatisfiedBy(_details).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnTrueWhenHasDayOff()
		{
			_scheduleDayFrom = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDayTo = MockRepository.GenerateMock<IScheduleDay>();
			_dayOff = MockRepository.GenerateMock<IDayOff>();
			_personAssignmentTo.Stub(x => x.DayOff()).Return(_dayOff);
			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()) { SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo };
			_details = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail };

			_target.IsSatisfiedBy(_details).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnTrueWhenHasAbsence()
		{
			_scheduleDayFrom = MockRepository.GenerateMock<IScheduleDay>();
			_scheduleDayTo = MockRepository.GenerateMock<IScheduleDay>();
			MockRepository.GenerateMock<IProjectionService>();
			_projectionServiceTo = MockRepository.GenerateMock<IProjectionService>();
			MockRepository.GenerateMock<IVisualLayer>();
			_visualLayerTo = MockRepository.GenerateMock<IVisualLayer>();
			MockRepository.GenerateMock<IVisualLayerCollection>();
			_visualLayerCollectionTo = MockRepository.GenerateMock<IVisualLayerCollection>();
			_scheduleDayTo.Stub(x => x.ProjectionService()).Return(_projectionServiceTo);
			_projectionServiceTo.Stub(x => x.CreateProjection()).Return(_visualLayerCollectionTo);
			_visualLayerCollectionTo.Stub(x => x.GetEnumerator())
				.Return(new List<IVisualLayer> { _visualLayerTo }.GetEnumerator());
			_visualLayerTo.Stub(x => x.Payload).Return(new Absence());

			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()) { SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo };
			_details = new List<IShiftTradeSwapDetail> { _shiftTradeSwapDetail };

			_target.IsSatisfiedBy(_details).Should().Be.EqualTo(true);
		}


		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			_target.IsSatisfiedBy(null);
		}
	}
}
