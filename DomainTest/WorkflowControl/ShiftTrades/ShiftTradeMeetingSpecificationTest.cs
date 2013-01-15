﻿using System;
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
		private IPersonAssignment _personAssignmentFrom;
		private IPersonAssignment _personAssignmentTo;
		private IMainShift _mainShift;
		private ILayerCollection<IActivity> _layerCollectionFrom;
		private ILayerCollection<IActivity> _layerCollectionTo;
		private DateTimePeriod? _periodFrom;
		private DateTimePeriod _periodTo;
		private ReadOnlyCollection<IPersonMeeting> _personMeetings;
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
			_personAssignmentFrom = _mock.StrictMock<IPersonAssignment>();
			_personAssignmentTo = _mock.StrictMock<IPersonAssignment>();
			_mainShift = _mock.StrictMock<IMainShift>();
			_layerCollectionFrom = _mock.StrictMock<ILayerCollection<IActivity>>();
			_layerCollectionTo = _mock.StrictMock<ILayerCollection<IActivity>>();
			var startFrom = new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var endFrom = new DateTime(2011, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			_periodFrom = new DateTimePeriod(startFrom, endFrom);
			_personMeeting = _mock.StrictMock<IPersonMeeting>();
			_personMeetings = new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{_personMeeting});
		}

		[Test]
		public void ShouldReturnDenyReason()
		{
			Assert.AreEqual("ShiftTradeMeetingSpecificationDenyReason", _target.DenyReason);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			_target.IsSatisfiedBy(null);
		}

		[Test]
		public void ShouldReturnTrueWhenShiftCoverMeeting()
		{
			var startTo = new DateTime(2011, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 12, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			using(_mock.Record())
			{
				Expect.Call(_scheduleDayFrom.AssignmentHighZOrder()).Return(_personAssignmentFrom);
				Expect.Call(_scheduleDayFrom.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
				Expect.Call(_personAssignmentFrom.MainShift).Return(_mainShift).Repeat.Twice();
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionFrom);
				Expect.Call(_layerCollectionFrom.Period()).Return(_periodFrom);

				Expect.Call(_scheduleDayTo.AssignmentHighZOrder()).Return(_personAssignmentTo);
				Expect.Call(_scheduleDayTo.PersonMeetingCollection()).Return(_personMeetings);
				Expect.Call(_personAssignmentTo.MainShift).Return(null);
				Expect.Call(_personMeeting.Period).Return(_periodTo);
			}

			using(_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfiedBy(_details));
			}
		}

		[Test]
		public void ShouldReturnFalseWhenShiftDoNotCoverMeeting()
		{
			var startTo = new DateTime(2011, 1, 1, 16, 0, 0, DateTimeKind.Utc);
			var endTo = new DateTime(2011, 1, 1, 18, 0, 0, DateTimeKind.Utc);
			_periodTo = new DateTimePeriod(startTo, endTo);

			using (_mock.Record())
			{
				Expect.Call(_scheduleDayTo.AssignmentHighZOrder()).Return(_personAssignmentTo);
				Expect.Call(_scheduleDayTo.PersonMeetingCollection()).Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
				Expect.Call(_personAssignmentTo.MainShift).Return(_mainShift).Repeat.Twice();
				Expect.Call(_mainShift.LayerCollection).Return(_layerCollectionTo);
				Expect.Call(_layerCollectionTo.Period()).Return(_periodFrom);

				Expect.Call(_scheduleDayFrom.AssignmentHighZOrder()).Return(_personAssignmentFrom);
				Expect.Call(_scheduleDayFrom.PersonMeetingCollection()).Return(_personMeetings);
				Expect.Call(_personAssignmentFrom.MainShift).Return(null);
				Expect.Call(_personMeeting.Period).Return(_periodTo);
			}

			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfiedBy(_details));
			}	
		}
	}
}
