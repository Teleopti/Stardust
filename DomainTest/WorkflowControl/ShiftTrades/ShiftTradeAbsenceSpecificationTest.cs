using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenInParameterIsNull()
		{
			_target.IsSatisfiedBy(null);
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

	[TestFixture]
	public class ShiftTradeShouldNotChangeStartDateTest
	{
		private Person _personFrom;
		private Person _personTo;
		private ShiftTradeShouldNotChangeStartDate _target;
		private DateTime _dateFrom;
		private DateTime _dateTime;
		private DateTimePeriod _periodFrom;
		private DateTimePeriod _periodTo;
		private SchedulePartFactoryForDomain _scheduleFactoryForSender;
		private SchedulePartFactoryForDomain _scheduleFactoryForReciever;

		[SetUp]
		public void Setup()
		{
			
		}


		[Test]
		public void IsSatsifiedBy_WhenBothSchedulesAreOnSameDayInSameTimeZone_ShouldBeTrue()
		{
			var target = new ShiftTradeShouldNotChangeStartDate();
			var personFrom = new Person(){Name=new Name("Ashley","Andeen")};
			var personTo = new Person(){Name=new Name("Bengt","Magnusson")};
			var date = new DateTime(2001, 1, 12);
			var scheduleFrom = scheduleFactory.ScheduleDayStub(date, personFrom);
			var scheduleTo = scheduleFactory.ScheduleDayStub(date,personTo);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(date), new DateOnly(date)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };
		
			Assert.That(target.IsSatisfiedBy(details));
		}

		[Test]
		public void IsSatisfiedBy_WhenScheduleToStartsOnDifferentDateThanScheduleFrom_ShouldBeFalse()
		{
			var target = new ShiftTradeShouldNotChangeStartDate();
			var personFrom = new Person() { Name = new Name("Ashley", "Andeen") };
			var personTo = new Person() { Name = new Name("Bengt", "Magnusson") };
			var dateFrom = new DateTime(2001, 1, 12);
			var dateTo = new DateTime(2001, 1, 13);
			var scheduleFrom = scheduleFactory.ScheduleDayStub(dateFrom, personFrom);
			var scheduleTo = scheduleFactory.ScheduleDayStub(dateTo, personTo);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(dateFrom), new DateOnly(dateTo)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(target.IsSatisfiedBy(details),Is.False);
		}

		[Test]
		public void IsSatisfiedBy_WhenTheNewShiftIsAnotherDateFromMyTimeZone_ShouldBeFalse()
		{

			_target = new ShiftTradeShouldNotChangeStartDate();
			_personFrom = new Person() { Name = new Name("Ashley", "Andeen") };
			_personTo = new Person() { Name = new Name("Bengt", "Magnusson") };
			_dateFrom = new DateTime(2001, 1, 12);
			_dateTime = new DateTime(2001, 1, 12);
			_periodFrom = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
			                                 new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));
			_periodTo = new DateTimePeriod(new DateTime(2001, 1, 11, 0, 0, 0, DateTimeKind.Utc),
			                               new DateTime(2001, 1, 11, 0, 0, 0, DateTimeKind.Utc));

			_scheduleFactoryForSender = new SchedulePartFactoryForDomain(_personFrom,_periodFrom);
			_scheduleFactoryForReciever = new SchedulePartFactoryForDomain(_personTo, _periodTo);

			_scheduleFactoryForSender.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x",TimeSpan.Zero,"x","x"));
			_scheduleFactoryForReciever.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(3), "x", "x"));
			
			var scheduleTo = _scheduleFactoryForReciever.AddMainShiftLayerBetween(TimeSpan.FromHours(22), TimeSpan.FromHours(23)).CreatePart(); 

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(_dateFrom), new DateOnly(_dateTime)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(_target.IsSatisfiedBy(details), Is.False);
		}

		[Test]
		public void IsSatisfiedBy_WhenScheduleToStartsOnDifferentDateThanScheduleFromAccordingToRecieversTimeZone_ShouldBeFalse()
		{
			var target = new ShiftTradeShouldNotChangeStartDate();
			var personFrom = new Person() { Name = new Name("Ashley", "Andeen") };
			var personTo = new Person() { Name = new Name("Bengt", "Magnusson") };
			var periodFrom = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
											new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));
			var periodTo = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
											new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));

			var scheduleFactoryForSender = new SchedulePartFactoryForDomain(personFrom, periodFrom);
			var scheduleFactoryForReciever = new SchedulePartFactoryForDomain(personTo, periodTo);

			scheduleFactoryForSender.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(-4), "x", "x"));
			scheduleFactoryForReciever.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(6), "x", "x"));

			var scheduleFrom = scheduleFactoryForSender.AddMainShiftLayerBetween(TimeSpan.FromHours(19), TimeSpan.FromHours(22)).CreatePart(); 
			var scheduleTo = scheduleFactoryForReciever.AddMainShiftLayerBetween(TimeSpan.FromHours(13), TimeSpan.FromHours(16)).CreatePart(); 

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, new DateOnly(periodFrom.StartDateTime), new DateOnly(periodTo.StartDateTime)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(target.IsSatisfiedBy(details), Is.False);
		}

	}
}
