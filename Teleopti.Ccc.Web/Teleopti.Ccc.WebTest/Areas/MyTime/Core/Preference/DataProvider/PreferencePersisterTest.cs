using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferencePersisterTest
	{
		[Test]
		public void ShouldAddPreference()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var input = new PreferenceDayInput();
			
			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

	    [Test]
	    public void ShouldAddMultiPreference()
	    {
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser, new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
				new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var input = new MultiPreferenceDaysInput()
			{
			    Dates = new List<DateOnly>{DateOnly.MinValue, DateOnly.Today, DateOnly.MaxValue}
			};

			target.PersistMultiDays(input);

			preferenceDayRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var input = new PreferenceDayInput();
			
			target.Persist(input).Should().Not.Be.Null();

			preferenceDayRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldUpdateExistingPreference()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var preferenceDay = new PreferenceDay(person,DateOnly.Today, new PreferenceRestriction());
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			
			target.Persist(input);

			preferenceDayRepository.AssertWasNotCalled(x => x.Add(preferenceDay));
		}

		[Test]
		public void ShouldDeleteOrphanPreferencesOnUpdate()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var preferenceDay1 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) { UpdatedOn = DateTime.Now.AddHours(-1) };
			var preferenceDay2 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) { UpdatedOn = DateTime.Now };
			var preferenceDay3 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) { UpdatedOn = DateTime.Now.AddHours(-2) };
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));

			preferenceDayRepository.Stub(x => x.Find(input.Date, null)).Return(new List<IPreferenceDay> { preferenceDay1, preferenceDay2, preferenceDay3 });

			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay1));
			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay3));
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			
			preferenceDayRepository.Stub(x => x.Find(input.Date, null)).Return(new List<IPreferenceDay> { preferenceDay });
			
			var result = target.Persist(input);

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay1 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var preferenceDay2 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));

			preferenceDayRepository.Stub(x => x.FindAndLock(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay1, preferenceDay2 });

			target.Delete(new List<DateOnly>() { DateOnly.Today });

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay1));
			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay2));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));

			preferenceDayRepository.Stub(x => x.FindAndLock(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay });

			var result = target.Delete(new List<DateOnly>() { DateOnly.Today });

			result.First().Value.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldClearExtendedPreference()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var preferenceRestriction = new PreferenceRestriction
			{
				StartTimeLimitation =
													new StartTimeLimitation(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
				EndTimeLimitation =
													new EndTimeLimitation(new TimeSpan(16, 0, 0), new TimeSpan(18, 0, 0)),
				WorkTimeLimitation =
													new WorkTimeLimitation(new TimeSpan(6, 0, 0), new TimeSpan(10, 0, 0))
			};
			preferenceRestriction.AddActivityRestriction(new ActivityRestriction(new Activity("Lunch")));
			preferenceDay.Stub(x => x.Restriction).Return(preferenceRestriction);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			
			target.Persist(input);

			preferenceDay.Restriction.StartTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.StartTimeLimitation.EndTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.EndTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.EndTimeLimitation.EndTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.WorkTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.WorkTimeLimitation.EndTime.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldClearMustHavesWhenAppyNewPreference()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var preferenceRestriction = new PreferenceRestriction
			{
				MustHave = true
			};
			preferenceDay.Stub(x => x.Restriction).Return(preferenceRestriction);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			
			target.Persist(input);

			preferenceDay.Restriction.MustHave.Should().Be.False();
		}

		[Test]
		public void ShouldClearTemplateName()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = new Person();
			var preferenceRestriction = new PreferenceRestriction();
			var preferenceDay = new PreferenceDay(person, DateOnly.Today, preferenceRestriction) { TemplateName = "Extended" };
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			
			target.Persist(input);

			preferenceDay.TemplateName.Should().Be.Null();
		}

		[Test]
		public void ShouldSetMustHave()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mustHaveRestrictionSetter,
				loggedOnUser,
				new PreferenceDayInputMapper(new FakeShiftCategoryRepository(), new FakeDayOffTemplateRepository(),
					new FakeAbsenceRepository(), new FakeActivityRepository(), loggedOnUser),
				new PreferenceDayViewModelMapper(new ExtendedPreferencePredicate()));
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = true };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			target.MustHave(input);
			mustHaveRestrictionSetter.AssertWasCalled(x => x.SetMustHave(input.Date, person, input.MustHave));
		}
	}
}



