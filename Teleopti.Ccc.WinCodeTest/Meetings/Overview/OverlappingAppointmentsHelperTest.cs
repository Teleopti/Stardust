using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
	[TestFixture]
	public class OverlappingAppointmentsHelperTest
	{
		private OverlappingAppointmentsHelper _target;
		private List<ISimpleAppointment> _appointments;

		[SetUp]
		public void Setup()
		{
			_target = new OverlappingAppointmentsHelper();

			var simple1 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 1, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0) };
			var simple2 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 2, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple1 };
			var simple3 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 3, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple2 };
			var simple4 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 4, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple3 };
			var simple5 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 5, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple4 };
			var simple6 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 6, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple5 };
			var simple7 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 7, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple5 };
			var simple8 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 8, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple5 };

			_appointments = new List<ISimpleAppointment> { simple1, simple2, simple3, simple4, simple5, simple6, simple7, simple8 };
		}

		[Test]
		public void ShouldWarnIfMoreThanFiveMeetingsOverlapping()
		{
			Assert.That(_target.HasTooManyOverlapping(_appointments), Is.True);
		}

		[Test]
		public void ShouldReduceOverlappingToFive()
		{
			var left = _target.ReduceOverlappingToFive(_appointments);
			Assert.That(left.Count,Is.LessThanOrEqualTo(5));
			Assert.That(left[0].OtherHasBeenDeleted,Is.True);
			Assert.That(left[1].OtherHasBeenDeleted,Is.True);
			Assert.That(left[2].OtherHasBeenDeleted,Is.True);
			Assert.That(left[3].OtherHasBeenDeleted,Is.True);
			Assert.That(left[4].OtherHasBeenDeleted,Is.True);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldReturnRemainsToHalfHour()
		{
			Assert.That(OverlappingAppointmentsHelper.FindRemainsToEvenHalfHour(20),Is.EqualTo(10));
			Assert.That(OverlappingAppointmentsHelper.FindRemainsToEvenHalfHour(29),Is.EqualTo(1));
			Assert.That(OverlappingAppointmentsHelper.FindRemainsToEvenHalfHour(1),Is.EqualTo(29));
			Assert.That(OverlappingAppointmentsHelper.FindRemainsToEvenHalfHour(40),Is.EqualTo(20));
			Assert.That(OverlappingAppointmentsHelper.FindRemainsToEvenHalfHour(0),Is.EqualTo(0));
		}
	}

}