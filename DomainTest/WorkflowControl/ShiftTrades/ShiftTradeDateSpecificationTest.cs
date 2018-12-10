using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
	[DomainTest]
	public class ShiftTradeDateSpecificationTest
	{
		private ShiftTradeDateSpecification _target;
		private Person _personFrom;
		private Person _personTo;

		[SetUp]
		public void Setup()
		{
			_target = new ShiftTradeDateSpecification();
			_personFrom = new Person().WithName(new Name("Ashley", "Andeen"));
			_personTo = new Person().WithName(new Name("Bengt", "Magnusson"));
		}

		[Test]
		public void IsSatsifiedBy_WhenBothSchedulesAreOnSameDayInSameTimeZone_ShouldBeTrue()
		{
			var date = new DateTime(2001, 1, 12);
			var scheduleFrom = new StubFactory().ScheduleDayStub(date, _personFrom);
			var scheduleTo = new StubFactory().ScheduleDayStub(date, _personTo);
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(date), new DateOnly(date)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };
		
			Assert.That(_target.IsSatisfiedBy(details));
		}

		[Test]
		public void IsSatisfiedBy_WhenScheduleToStartsOnDifferentDateThanScheduleFrom_ShouldBeFalse()
		{
			var dateFrom = new DateTime(2001, 1, 12,0,0,0,DateTimeKind.Utc);
			var dateTo = new DateTime(2001, 1, 13, 0, 0, 0, DateTimeKind.Utc);
			var scheduleFactoryForSender = new SchedulePartFactoryForDomain(_personFrom, new DateTimePeriod(dateFrom,dateFrom));
			var scheduleFactoryForReciever = new SchedulePartFactoryForDomain(_personTo, new DateTimePeriod(dateTo,dateTo));
			var scheduleFrom = scheduleFactoryForSender.CreatePartWithMainShift();
			var scheduleTo = scheduleFactoryForReciever.CreatePartWithMainShift();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(dateFrom), new DateOnly(dateTo)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(_target.IsSatisfiedBy(details),Is.False);
		}

		[Test]
		public void IsSatisfiedBy_WhenTheNewShiftIsAnotherDateFromAccordingToSendersTimeZone_ShouldBeFalse()
		{

			var dateFrom = new DateTime(2001, 1, 12);
			var dateTime = new DateTime(2001, 1, 12);
			var periodFrom = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
			                                    new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));
			var periodTo = new DateTimePeriod(new DateTime(2001, 1, 11, 0, 0, 0, DateTimeKind.Utc),
			                                  new DateTime(2001, 1, 11, 0, 0, 0, DateTimeKind.Utc));

			var scheduleFactoryForSender = new SchedulePartFactoryForDomain(_personFrom,periodFrom);
			var scheduleFactoryForReciever = new SchedulePartFactoryForDomain(_personTo, periodTo);

			scheduleFactoryForSender.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x",TimeSpan.Zero,"x","x"));
			scheduleFactoryForReciever.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(3), "x", "x"));
			
			var scheduleFrom = scheduleFactoryForReciever.AddMainShiftLayerBetween(TimeSpan.FromHours(1), TimeSpan.FromHours(2)).CreatePart();
			var scheduleTo = scheduleFactoryForSender.AddMainShiftLayerBetween(TimeSpan.FromHours(22), TimeSpan.FromHours(23)).CreatePart(); 

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(dateFrom), new DateOnly(dateTime)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(_target.IsSatisfiedBy(details), Is.False);
		}

		[Test]
		public void IsSatisfiedBy_WhenScheduleToStartsOnDifferentDateThanScheduleFromAccordingToRecieversTimeZone_ShouldBeFalse()
		{
			var periodFrom = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
			                                    new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));
			var periodTo = new DateTimePeriod(new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc),
			                                  new DateTime(2001, 1, 12, 0, 0, 0, DateTimeKind.Utc));

			var scheduleFactoryForSender = new SchedulePartFactoryForDomain(_personFrom, periodFrom);
			var scheduleFactoryForReciever = new SchedulePartFactoryForDomain(_personTo, periodTo);

			scheduleFactoryForSender.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(-4), "x", "x"));
			scheduleFactoryForReciever.CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.CreateCustomTimeZone("x", TimeSpan.FromHours(6), "x", "x"));

			var scheduleFrom = scheduleFactoryForSender.AddMainShiftLayerBetween(TimeSpan.FromHours(19), TimeSpan.FromHours(22)).CreatePart(); 
			var scheduleTo = scheduleFactoryForReciever.AddMainShiftLayerBetween(TimeSpan.FromHours(13), TimeSpan.FromHours(16)).CreatePart(); 

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(periodFrom.StartDateTime), new DateOnly(periodTo.StartDateTime)) { SchedulePartFrom = scheduleFrom, SchedulePartTo = scheduleTo };
			var details = new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail };

			Assert.That(_target.IsSatisfiedBy(details), Is.False);
		}

	}
}