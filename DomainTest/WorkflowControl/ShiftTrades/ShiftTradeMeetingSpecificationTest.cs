using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[TestFixture]
	public class ShiftTradeMeetingSpecificationTest
	{
		private ShiftTradeMeetingSpecification _target;
		private MockRepository _mock;
		private IList<IShiftTradeSwapDetail> _details;
		private IShiftTradeSwapDetail _swapDetail;
		private IPerson _personFrom;
		private IPerson _personTo;
		private IScheduleDay _scheduleDayFrom;
		private IScheduleDay _scheduleDayTo;
		private IEditableShift _mainShift;
		private List<IEditableShiftLayer> _layerCollectionFrom;
		private List<IEditableShiftLayer> _layerCollectionTo;
		private DateTimePeriod _periodFrom;
		private DateTimePeriod _periodTo;
		private IPersonMeeting[] _personMeetings;
		private IPersonMeeting _personMeeting;
			
		[SetUp]
		public void Setup()
		{
			_target = new ShiftTradeMeetingSpecification();
			_mock = new MockRepository();
			_personFrom = _mock.StrictMock<IPerson>();
			_personTo = _mock.StrictMock<IPerson>();
			_scheduleDayFrom = _mock.StrictMock<IScheduleDay>();
			_scheduleDayTo = _mock.StrictMock<IScheduleDay>();
			_swapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(), new DateOnly()){SchedulePartFrom = _scheduleDayFrom, SchedulePartTo = _scheduleDayTo};
			_details = new List<IShiftTradeSwapDetail>{_swapDetail};
			_mainShift = _mock.StrictMock<IEditableShift>();
			_layerCollectionFrom = new List<IEditableShiftLayer>();
			_layerCollectionTo = new List<IEditableShiftLayer>();
			var startFrom = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endFrom = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			_periodFrom = new DateTimePeriod(startFrom, endFrom);
			_personMeeting = _mock.StrictMock<IPersonMeeting>();
			_personMeetings = new[]{_personMeeting};
		}

		[Test]
		public void ShouldReturnDenyReason()
		{
			Assert.AreEqual("ShiftTradeMeetingSpecificationDenyReason", _target.DenyReason);
		}

		[Test]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => _target.IsSatisfiedBy(null));
		}

		[Test]
		public void ShouldReturnTrueWhenShiftCoverMeeting()
		{
			var startTo = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);
			_layerCollectionFrom.Add(new EditableShiftLayer(new Activity("ds"), _periodFrom));

			using(_mock.Record())
			{
				Expect.Call(_scheduleDayFrom.PersonMeetingCollection()).Return(new IPersonMeeting[0]);
				Expect.Call(_scheduleDayFrom.GetEditorShift()).Return(_mainShift);
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionFrom);

				Expect.Call(_scheduleDayTo.PersonMeetingCollection()).Return(_personMeetings);
				Expect.Call(_scheduleDayTo.GetEditorShift()).Return(null);
				Expect.Call(_personMeeting.Period).Return(_periodTo);
			}

			using(_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfiedBy(_details));
			}
		}


		[Test]
		public void ShouldReturnFalseWhenShiftWithHoleDoNotCoverMeeting()
		{
			var startTo = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			_layerCollectionTo.Add(new EditableShiftLayer(new Activity("ds"), new DateTimePeriod(_periodFrom.StartDateTime, _periodFrom.StartDateTime.AddHours(1))));
			_layerCollectionTo.Add(new EditableShiftLayer(new Activity("ds"), new DateTimePeriod(_periodFrom.EndDateTime.AddHours(-1), _periodFrom.EndDateTime)));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDayTo.PersonMeetingCollection()).Return(new IPersonMeeting[0]);
				Expect.Call(_scheduleDayTo.GetEditorShift()).Return(_mainShift);
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionTo);

				Expect.Call(_scheduleDayFrom.PersonMeetingCollection()).Return(_personMeetings);
				Expect.Call(_scheduleDayFrom.GetEditorShift()).Return(null);
				Expect.Call(_personMeeting.Period).Return(_periodTo);
			}

			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));
			}
		}

		[Test]
		public void ShouldReturnFalseWhenShiftDoNotCoverMeeting()
		{
			var startTo = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			_layerCollectionTo.Add(new EditableShiftLayer(new Activity("ds"), _periodFrom));

			using (_mock.Record())
			{
				Expect.Call(_scheduleDayTo.PersonMeetingCollection()).Return(new IPersonMeeting[0]);
				Expect.Call(_scheduleDayTo.GetEditorShift()).Return(_mainShift);
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionTo);

				Expect.Call(_scheduleDayFrom.PersonMeetingCollection()).Return(_personMeetings);
				Expect.Call(_scheduleDayFrom.GetEditorShift()).Return(null);
				Expect.Call(_personMeeting.Period).Return(_periodTo);
			}

			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));
			}	
		}
	}
}
