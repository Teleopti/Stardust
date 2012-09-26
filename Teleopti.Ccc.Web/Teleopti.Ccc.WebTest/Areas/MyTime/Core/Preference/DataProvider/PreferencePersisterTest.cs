﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferencePersisterTest
	{
		[Test]
		public void ShouldAddPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var input = new PreferenceDayInput();

			mapper.Stub(x => x.Map<PreferenceDayInput, IPreferenceDay>(input)).Return(preferenceDay);

			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Add(preferenceDay));
			mapper.AssertWasNotCalled(x => x.Map<PreferenceDayInput, IPreferenceDay>(input, null));
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(MockRepository.GenerateMock<IPreferenceDayRepository>(), mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var input = new PreferenceDayInput();
			var inputResult = new PreferenceDayViewModel();

			mapper.Stub(x => x.Map<PreferenceDayInput, IPreferenceDay>(input)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay)).Return(inputResult);

			var result = target.Persist(input);

			result.Should().Be.SameInstanceAs(inputResult);
		}

		[Test]
		public void ShouldUpdateExistingPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			mapper.AssertWasCalled(x => x.Map(input, preferenceDay));
			preferenceDayRepository.AssertWasNotCalled(x => x.Add(preferenceDay));
		}

		[Test]
		public void ShouldDeleteOrphanPreferencesOnUpdate()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var preferenceDay1 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) {UpdatedOn = DateTime.Now.AddHours(-1)};
			var preferenceDay2 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) { UpdatedOn = DateTime.Now };
			var preferenceDay3 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction()) {UpdatedOn = DateTime.Now.AddHours(-2)};
			var target = new PreferencePersister(preferenceDayRepository, MockRepository.GenerateMock<IMappingEngine>(), MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(input.Date, null)).Return(new List<IPreferenceDay> { preferenceDay1, preferenceDay2, preferenceDay3 });

			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay1));
			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay3));
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var target = new PreferencePersister(preferenceDayRepository, mapper, MockRepository.GenerateMock<ILoggedOnUser>());
			var viewModel = new PreferenceDayViewModel();

			preferenceDayRepository.Stub(x => x.Find(input.Date, null)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay)).Return(viewModel);

			var result = target.Persist(input);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay1 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var preferenceDay2 = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay1, preferenceDay2 });

			target.Delete(DateOnly.Today);

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay1));
			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay2));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay });

			var result = target.Delete(DateOnly.Today);

			result.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldThrowHttp404OIfPreferenceDoesNotExists()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var target = new PreferencePersister(preferenceDayRepository, null, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, null)).Return(new List<IPreferenceDay>());

			var exception = Assert.Throws<HttpException>(() => target.Delete(DateOnly.Today));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldClearExtendedPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
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
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			preferenceDay.Restriction.StartTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.StartTimeLimitation.EndTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.EndTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.EndTimeLimitation.EndTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.WorkTimeLimitation.StartTime.HasValue.Should().Be.False();
			preferenceDay.Restriction.WorkTimeLimitation.EndTime.HasValue.Should().Be.False();
		}

		[Test]
		public void ShouldNotClearMustHaves()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var preferenceRestriction = new PreferenceRestriction
			                            	{
				MustHave = true
			};
			preferenceDay.Stub(x => x.Restriction).Return(preferenceRestriction);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			preferenceDay.Restriction.MustHave.Should().Be.True();
		}

		[Test]
		public void ShouldClearTemplateName()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = new Person();
			var preferenceRestriction = new PreferenceRestriction();
			var preferenceDay = new PreferenceDay(person, DateOnly.Today, preferenceRestriction) {TemplateName = "Extended"};
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			preferenceDay.TemplateName.Should().Be.Null();
		}
		
		[Test]
		public void ShouldUpdateMustHaveWithoutClearPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = false };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var preferenceRestriction = new PreferenceRestriction
				{
					ShiftCategory = new ShiftCategory("Late")
				};
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today); 
			preferenceDay.Stub(x => x.Restriction).Return(preferenceRestriction);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			preferenceDay.Restriction.MustHave = true;
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.MustHave(period, input);

			preferenceDay.Restriction.MustHave.Should().Be.False();
			preferenceDay.Restriction.ShiftCategory.Description.Name.Should().Be.EqualTo("Late");
		}

		[Test]
		public void ShouldNotUpdateMustHaveWhenPreferenceEmpty()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = false };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay>());

			target.MustHave(period, input);

			preferenceDayRepository.AssertWasNotCalled(x => x.Add(preferenceDay));
		}

		[Test]
		public void ShouldAddMustHaveGivenUnderLimit()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			var restriction =  MockRepository.GenerateMock<IPreferenceRestriction>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var schedulePeriod = MockRepository.GenerateMock<ISchedulePeriod>();
			var input = new MustHaveInput {Date = DateOnly.Today, MustHave = true};
			
			const int limit = 2;

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(period, person)).Return(new List<IPreferenceDay> { preferenceDay });
			preferenceDay.Stub(x => x.Restriction).Return(restriction);
			preferenceDay.Stub(x => x.RestrictionDate).Return(DateOnly.Today);
			person.Stub(x => x.SchedulePeriod(DateOnly.Today)).Return(schedulePeriod);
			schedulePeriod.Stub(x => x.MustHavePreference).Return(limit);
			
			target.MustHave(period, input);
			restriction.AssertWasCalled(x => x.MustHave = true);
		}


		[Test]
		public void ShouldNotAddMustHaveGivenOverLimit()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			var restriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var schedulePeriod = MockRepository.GenerateMock<ISchedulePeriod>();
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = true };

			const int limit = 1;

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(period, person)).Return(new List<IPreferenceDay>
				{preferenceDay, preferenceDay});
			preferenceDay.Stub(x => x.Restriction).Return(restriction);
			restriction.Stub(x => x.MustHave).Return(true);
			person.Stub(x => x.SchedulePeriod(DateOnly.Today)).Return(schedulePeriod);
			schedulePeriod.Stub(x => x.MustHavePreference).Return(limit);

			target.MustHave(period, input);
			restriction.AssertWasNotCalled(x => x.MustHave = true);
		}


		[Test]
		public void ShouldNotAddMustHaveGivenEqualToLimit()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			var restriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var schedulePeriod = MockRepository.GenerateMock<ISchedulePeriod>();
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = true };

			const int limit = 2;

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(period, person)).Return(new List<IPreferenceDay>
				{preferenceDay, preferenceDay});
			preferenceDay.Stub(x => x.Restriction).Return(restriction);
			restriction.Stub(x => x.MustHave).Return(true);
			person.Stub(x => x.SchedulePeriod(DateOnly.Today)).Return(schedulePeriod);
			schedulePeriod.Stub(x => x.MustHavePreference).Return(limit);

			target.MustHave(period, input);
			restriction.AssertWasNotCalled(x => x.MustHave = true);
		}

		[Test]
		public void ShouldRemoveMustHaveGivenEqualToLimit()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, loggedOnUser);
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(7));
			var restriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var schedulePeriod = MockRepository.GenerateMock<ISchedulePeriod>();
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = false };

			const int limit = 1;

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(DateOnly.Today, person)).Return(new List<IPreferenceDay> { preferenceDay });
			preferenceDay.Stub(x => x.Restriction).Return(restriction);
			restriction.Stub(x => x.MustHave).Return(true);
			person.Stub(x => x.SchedulePeriod(DateOnly.Today)).Return(schedulePeriod);
			schedulePeriod.Stub(x => x.MustHavePreference).Return(limit);

			target.MustHave(period, input);
			restriction.AssertWasCalled(x => x.MustHave = false);
		}
	}
}



