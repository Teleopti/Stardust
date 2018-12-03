using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeAbsenceSpecificationTest
	{
		private ShiftTradeAbsenceSpecification _target;
		private MockRepository _mocks;
		private IShiftTradeSwapDetail _shiftTradeSwapDetail;
		private IPerson _personFrom;
		private IPerson _personTo;
		private IScheduleDay _scheduleDayFrom;
		private IScheduleDay _scheduleDayTo;
		private IProjectionService _projectionServiceFrom;
		private IProjectionService _projectionServiceTo;
		private IVisualLayerCollection _visualLayerCollectionFrom;
		private IVisualLayerCollection _visualLayerCollectionTo;
		private IVisualLayer _visualLayerFrom;
		private IVisualLayer _visualLayerTo;
		private IList<IShiftTradeSwapDetail> _details;

		[SetUp]
		public void Setup()
		{
			_target = new ShiftTradeAbsenceSpecification();
			_mocks = new MockRepository();
			_personFrom = _mocks.StrictMock<IPerson>();
			_personTo = _mocks.StrictMock<IPerson>();
			_scheduleDayFrom = _mocks.StrictMock<IScheduleDay>();
			_scheduleDayTo = _mocks.StrictMock<IScheduleDay>();
			_projectionServiceFrom = _mocks.StrictMock<IProjectionService>();
			_projectionServiceTo = _mocks.StrictMock<IProjectionService>();
			_visualLayerFrom = _mocks.StrictMock<IVisualLayer>();
			_visualLayerTo = _mocks.StrictMock<IVisualLayer>();
			_visualLayerCollectionFrom = _mocks.StrictMock<IVisualLayerCollection>();
			_visualLayerCollectionTo = _mocks.StrictMock<IVisualLayerCollection>();
			_shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()){SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo};
			_details = new List<IShiftTradeSwapDetail>{_shiftTradeSwapDetail};
		}

		[Test]
		public void ShouldReturnDenyReason()
		{
			Assert.AreEqual("ShiftTradeAbsenceDenyReason", _target.DenyReason);
		}

		[Test]
		public void ShouldReturnTrueWhenNoAbsenceInShifts()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDayFrom.ProjectionService()).Return(_projectionServiceFrom);
				Expect.Call(_scheduleDayTo.ProjectionService()).Return(_projectionServiceTo);
				Expect.Call(_projectionServiceFrom.CreateProjection()).Return(_visualLayerCollectionFrom);
				Expect.Call(_projectionServiceTo.CreateProjection()).Return(_visualLayerCollectionTo);
				Expect.Call(_visualLayerCollectionFrom.GetEnumerator()).Return(new List<IVisualLayer> {_visualLayerFrom}.GetEnumerator());
				Expect.Call(_visualLayerCollectionTo.GetEnumerator()).Return(new List<IVisualLayer> { _visualLayerTo }.GetEnumerator());
				Expect.Call(_visualLayerFrom.Payload).Return(new Activity("activity"));
				Expect.Call(_visualLayerTo.Payload).Return(new Activity("activity"));
			}

			using(_mocks.Playback())
			{
				Assert.IsTrue(_target.IsSatisfiedBy(_details));
			}
		}

		[Test]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target.IsSatisfiedBy(null));
		}

		[Test]
		public void ShouldReturnFalseWhenAbsenceInTheFromShift()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDayFrom.ProjectionService()).Return(_projectionServiceFrom);
				Expect.Call(_projectionServiceFrom.CreateProjection()).Return(_visualLayerCollectionFrom);
				Expect.Call(_visualLayerCollectionFrom.GetEnumerator()).Return(new List<IVisualLayer> {_visualLayerFrom}.GetEnumerator());
				Expect.Call(_visualLayerFrom.Payload).Return(new Absence());
			}

			using (_mocks.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));	
			}
		}

		[Test]
		public void ShouldReturnFalseWhenAbsenceInTheToShift()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDayFrom.ProjectionService()).Return(_projectionServiceFrom);
				Expect.Call(_projectionServiceFrom.CreateProjection()).Return(_visualLayerCollectionFrom);
				Expect.Call(_visualLayerCollectionFrom.GetEnumerator()).Return(new List<IVisualLayer> {_visualLayerFrom}.GetEnumerator());
				Expect.Call(_visualLayerFrom.Payload).Return(new Activity("activity"));

				Expect.Call(_scheduleDayTo.ProjectionService()).Return(_projectionServiceTo);
				Expect.Call(_projectionServiceTo.CreateProjection()).Return(_visualLayerCollectionTo);
				Expect.Call(_visualLayerCollectionTo.GetEnumerator()).Return(new List<IVisualLayer> {_visualLayerTo}.GetEnumerator());
				Expect.Call(_visualLayerTo.Payload).Return(new Absence());
			}
			
			using(_mocks.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));
			}
		}
	}
}
