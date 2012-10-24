using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"), TestFixture]
	public class MeetingRestrictionTest
	{
		[Test]
		public static void ShouldMatchShifts()
		{
			var target = new MeetingRestriction(null);

			target.MayMatchWithShifts().Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchBlacklistedShifts()
		{
			var target = new MeetingRestriction(null);

			target.MayMatchBlacklistedShifts().Should().Be.False();
		}

		[Test]
		public static void ShouldMatchWithAnyShiftCategory()
		{
			var target = new MeetingRestriction(null);

			target.Match(new ShiftCategory(" ")).Should().Be.True();
		}

		[Test]
		public static void ShouldMatchWithMeetingsOutsideShifts()
		{
			var meeting = new PersonMeeting(null, new MeetingPerson(new Person(), false), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(3), DateTime.UtcNow.Date.AddHours(4)));
			var target = new MeetingRestriction(new[] { meeting });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(9), WorkShift.BaseDate.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.True();
		}

		[Test]
		public static void ShouldMatchWithActivitiesWhereMeetingsIsAllowed()
		{
			var meeting = new PersonMeeting(null, new MeetingPerson(new Person(), false), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(10), DateTime.UtcNow.Date.AddHours(11)));
			var target = new MeetingRestriction(new[] { meeting });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(9), WorkShift.BaseDate.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchWithActivitiesWhereMeetingsIsDisallowed()
		{
			var meeting = new PersonMeeting(null, new MeetingPerson(new Person(), false), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(11), DateTime.UtcNow.Date.AddHours(12)));
			var target = new MeetingRestriction(new[] { meeting });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(9), WorkShift.BaseDate.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchWithIntersectingActivitiesWhereMeetingsIsDisallowed()
		{
			var meeting = new PersonMeeting(null, new MeetingPerson(new Person(), false), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(6), DateTime.UtcNow.Date.AddHours(10)));
			var target = new MeetingRestriction(new[] { meeting });
			var shift = new WorkShiftProjection
			{
				Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(6), WorkShift.BaseDate.AddHours(9))
								},
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(9), WorkShift.BaseDate.AddHours(17))
								}
						}
			};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchWithOverlappingActivitiesWhereMeetingsIsDisallowed()
		{
			var meeting = new PersonMeeting(null, new MeetingPerson(new Person(), false), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(6), DateTime.UtcNow.Date.AddHours(19)));
			var target = new MeetingRestriction(new[] { meeting });
			var shift = new WorkShiftProjection
			{
				Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(6), WorkShift.BaseDate.AddHours(9))
								},
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(WorkShift.BaseDate.AddHours(9), WorkShift.BaseDate.AddHours(17))
								}
						}
			};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldGetHash()
		{
			var person = new Person();
			var meetingPerson = new MeetingPerson(person, true);
			var meeting = new PersonMeeting(new Meeting(person, new[] { meetingPerson }, " ", " ", " ", new Activity(" "), new Scenario(" ")), meetingPerson, new DateTimePeriod(DateTime.UtcNow.Date.AddHours(8), DateTime.UtcNow.Date.AddHours(19)));
			var target = new MeetingRestriction(new[] { meeting, meeting });

			target.GetHashCode().Should().Be.EqualTo((meeting.GetHashCode() * 398) ^ meeting.GetHashCode());
		}

	}
}