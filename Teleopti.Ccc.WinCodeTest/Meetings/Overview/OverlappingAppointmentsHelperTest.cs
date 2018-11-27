using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;

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
		public void ShouldReduceOverlappingToFive()
		{
			var left = _target.ReduceOverlappingToFive(_appointments);
			Assert.That(left.Count(),Is.EqualTo(5));
			Assert.That(left.ElementAt(0).OtherHasBeenDeleted,Is.True);
			Assert.That(left.ElementAt(1).OtherHasBeenDeleted,Is.True);
			Assert.That(left.ElementAt(2).OtherHasBeenDeleted,Is.True);
			Assert.That(left.ElementAt(3).OtherHasBeenDeleted,Is.True);
			Assert.That(left.ElementAt(4).OtherHasBeenDeleted,Is.True);
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

		[Test]
		public void ShoulConsiderMoreThanPreviousMeetingWhenReducingOverlappingToFive()
		{
			_target = new OverlappingAppointmentsHelper();

			var simple1 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 16, 0, 0) };
			var simple2 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 9, 30, 0), EndDateTime = new DateTime(2011, 9, 13, 10, 0, 0), PreviousAppointment = simple1 };
			var simple3 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 10, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 11, 0, 0), PreviousAppointment = simple2 };
			var simple4 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 10, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 11, 0, 0), PreviousAppointment = simple3 };
			var simple5 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 10, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 11, 0, 0), PreviousAppointment = simple4 };
			var simple6 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 10, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 11, 0, 0), PreviousAppointment = simple5 };
			var simple7 = new SimpleAppointment { StartDateTime = new DateTime(2011, 9, 13, 10, 0, 0), EndDateTime = new DateTime(2011, 9, 13, 11, 0, 0), PreviousAppointment = simple6 };
			

			_appointments = new List<ISimpleAppointment> { simple1, simple2, simple3, simple4, simple5, simple6, simple7};

			var reduced = _target.ReduceOverlappingToFive(_appointments);
			//five overlapping each other + one that only overlapps one appointment(9:30 - 10:00 only overlapp with 9:00 - 16:00)
			Assert.AreEqual(6, reduced.Count());
		}
	}
}