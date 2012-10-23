using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"), TestFixture]
	public class PersonalShiftRestrictionTest
	{
		[Test]
		public static void ShouldMatchShifts()
		{
			var target = new PersonalShiftRestriction(null);

			target.MayMatchWithShifts().Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchBlacklistedShifts()
		{
			var target = new PersonalShiftRestriction(null);

			target.MayMatchBlacklistedShifts().Should().Be.False();
		}

		[Test]
		public static void ShouldMatchWithAnyShiftCategory()
		{
			var target = new PersonalShiftRestriction(null);

			target.Match(new ShiftCategory(" ")).Should().Be.True();
		}

		[Test]
		public static void ShouldMatchWithPersonalShiftOutsideShifts()
		{
			var personalShiftActivityLayer = new PersonalShiftActivityLayer(new Activity(" "), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(8), DateTime.UtcNow.Date.AddHours(9)));
			var personAssignment = new StubFactory().PersonAssignmentPersonalShiftStub(personalShiftActivityLayer);
			var target = new PersonalShiftRestriction(new[] { personAssignment });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.True();
		}

		[Test]
		public static void ShouldMatchWithActivitiesWherePersonalShiftIsAllowed()
		{
			var personalShiftActivityLayer = new PersonalShiftActivityLayer(new Activity(" "), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(10), DateTime.UtcNow.Date.AddHours(11)));
			var personAssignment = new StubFactory().PersonAssignmentPersonalShiftStub(personalShiftActivityLayer);
			var target = new PersonalShiftRestriction(new[] { personAssignment });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.True();
		}

		[Test]
		public static void ShouldNotMatchWithActivitiesWherePersonalShiftIsDisallowed()
		{
			var personalShiftActivityLayer = new PersonalShiftActivityLayer(new Activity(" "), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(10), DateTime.UtcNow.Date.AddHours(11)));
			var personAssignment = new StubFactory().PersonAssignmentPersonalShiftStub(personalShiftActivityLayer);
			var target = new PersonalShiftRestriction(new[] { personAssignment });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchWithIntersectingActivitiesWherePersonalShiftIsDisallowed()
		{
			var personalShiftActivityLayer = new PersonalShiftActivityLayer(new Activity(" "), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(8), DateTime.UtcNow.Date.AddHours(10)));
			var personAssignment = new StubFactory().PersonAssignmentPersonalShiftStub(personalShiftActivityLayer);
			var target = new PersonalShiftRestriction(new[] { personAssignment });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(7), DateTime.UtcNow.Date.AddHours(9))
								},
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldNotMatchWithOverlappingActivitiesWherePersonalShiftIsDisallowed()
		{
			var personalShiftActivityLayer = new PersonalShiftActivityLayer(new Activity(" "), new DateTimePeriod(DateTime.UtcNow.Date.AddHours(8), DateTime.UtcNow.Date.AddHours(19)));
			var personAssignment = new StubFactory().PersonAssignmentPersonalShiftStub(personalShiftActivityLayer);
			var target = new PersonalShiftRestriction(new[] { personAssignment });
			var shift = new WorkShiftProjection
				{
					Layers = new[]
						{
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = true,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(7), DateTime.UtcNow.Date.AddHours(9))
								},
							new WorkShiftProjectionLayer
								{
									ActivityAllowsOverwrite = false,
									Period = new DateTimePeriod(DateTime.UtcNow.Date.AddHours(9), DateTime.UtcNow.Date.AddHours(17))
								}
						}
				};

			var result = target.Match(shift);

			result.Should().Be.False();
		}

		[Test]
		public static void ShouldGetHash()
		{
			var personAssignment = new PersonAssignment(new Person(), new Scenario(" "));
			var target = new PersonalShiftRestriction(new IPersonAssignment[] {personAssignment, personAssignment});

			target.GetHashCode().Should().Be.EqualTo((personAssignment.GetHashCode() * 398) ^ personAssignment.GetHashCode());
		}
	}
}