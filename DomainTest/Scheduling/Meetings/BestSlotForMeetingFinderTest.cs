using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;


namespace Teleopti.Ccc.DomainTest.Scheduling.Meetings
{
	[TestFixture]
	public class BestSlotForMeetingFinderTest
	{
		private BestSlotForMeetingFinder _target;
		private IMeetingSlotImpactCalculator _meetingSlotImpactCalculator;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_meetingSlotImpactCalculator = _mocks.StrictMock<IMeetingSlotImpactCalculator>();
			_target = new BestSlotForMeetingFinder(_meetingSlotImpactCalculator);
		}

		[Test]
		public void ShouldCallImpactCalculatorForEachSearchSlot()
		{
			var startDateTime = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var persons = new List<IPerson>();
			var searchPeriod = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(120));
			var meetingPeriod = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(60));

			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod)).Return(1);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(15)))).Return(2);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(30)))).Return(2);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(45)))).Return(2);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(60)))).Return(2);
			_mocks.ReplayAll();

			_target.FindBestSlot(persons, searchPeriod, TimeSpan.FromMinutes(60), 15);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnASortedListOfSlotsWithBestFirst()
		{
			var startDateTime = new DateTime(2010, 11, 1, 11, 0, 0, DateTimeKind.Utc);
			var persons = new List<IPerson>();
			var searchPeriod = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(120));
			var meetingPeriod = new DateTimePeriod(startDateTime, startDateTime.AddMinutes(60));

			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod)).Return(1);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(15)))).Return(5);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(30)))).Return(null);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(45)))).Return(10);
			Expect.Call(_meetingSlotImpactCalculator.GetImpact(persons, meetingPeriod.MovePeriod(TimeSpan.FromMinutes(60)))).Return(8);
			_mocks.ReplayAll();

			var result = _target.FindBestSlot(persons, searchPeriod, TimeSpan.FromMinutes(60), 15);

			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result[0].SlotValue ,Is.EqualTo(10));
			_mocks.VerifyAll();
		}
	}

	

	
}
