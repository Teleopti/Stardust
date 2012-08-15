using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class ExtendedPreferencePredicateTest
	{
		[Test]
		public void ShouldNotSayExtendedOfNewEntity()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());

			target.IsExtended(preferenceDay).Should().Be.False();
		}

		[Test]
		public void ShouldNotSayExtendedOfAbsence()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						Absence = new Absence()
					});

			target.IsExtended(preferenceDay).Should().Be.False();
		}

		[Test]
		public void ShouldNotSayExtendedOfDayOff()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						DayOffTemplate = new DayOffTemplate(new Description())
					});

			target.IsExtended(preferenceDay).Should().Be.False();
		}

		[Test]
		public void ShouldNotSayExtendedOfShiftCategory()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						ShiftCategory = new ShiftCategory(" ")
					});

			target.IsExtended(preferenceDay).Should().Be.False();
		}

		[Test]
		public void ShouldSayExtendedOfWorkTimeLimitation()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2))
					});

			target.IsExtended(preferenceDay).Should().Be.True();
		}

		[Test]
		public void ShouldSayExtendedOfStartTimeLimitation()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2))
					});

			target.IsExtended(preferenceDay).Should().Be.True();
		}

		[Test]
		public void ShouldSayExtendedOfEndTimeLimitation()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				new PreferenceRestriction
					{
						EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2))
					});

			target.IsExtended(preferenceDay).Should().Be.True();
		}

		[Test]
		public void ShouldSayExtendedOfActivityRestrictions()
		{
			var target = new ExtendedPreferencePredicate();

			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.AddActivityRestriction(new ActivityRestriction(new Activity(" ")));
			var preferenceDay = new PreferenceDay(
				new Person(), DateOnly.Today,
				preferenceRestriction);

			target.IsExtended(preferenceDay).Should().Be.True();
		}

	}
}
