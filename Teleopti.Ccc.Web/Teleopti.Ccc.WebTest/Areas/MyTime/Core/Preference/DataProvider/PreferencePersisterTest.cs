﻿using System;
using System.Collections.Generic;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class OvertimeAvailabilityPersisterTest
	{
		[Test]
		public void ShouldAddOvertimeAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var input = new OvertimeAvailabilityInput();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person)).Return(null);
			var overtimeAvailability = new OvertimeAvailability(person, input.Date, input.StartTime.ToTimeSpan(), input.EndTime.ToTimeSpan());
			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
			      .Return(overtimeAvailability);
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository,
			                                               loggedOnUser, mapper);
			target.Persist(input);

			overtimeAvailabilityRepository.AssertWasCalled(x => x.Add(overtimeAvailability));
			mapper.AssertWasNotCalled(x => x.Map(input, (IOvertimeAvailability)null));
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var overtimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			var input = new OvertimeAvailabilityInput();

			mapper.Stub(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input))
				  .Return(overtimeAvailability);
			var viewModel = new OvertimeAvailabilityViewModel();
			mapper.Stub(x => x.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(overtimeAvailability))
			      .Return(viewModel);

			var result = target.Persist(input);
			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldUpdateExistingOvertimeAvailability()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var input = new OvertimeAvailabilityInput();
			var overtimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person))
			                              .Return(new List<IOvertimeAvailability>
				                              {
					                              overtimeAvailability
				                              });

			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			target.Persist(input);

			mapper.AssertWasCalled(x => x.Map(input, overtimeAvailability));
			mapper.AssertWasNotCalled(x => x.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(input));
		}

		[Test]
		public void ShouldReturnFormResultModelOnUpdate()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var overtimeAvailabilityRepository = MockRepository.GenerateMock<IOvertimeAvailabilityRepository>();
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var input = new OvertimeAvailabilityInput();
			var target = new OvertimeAvailabilityPersister(overtimeAvailabilityRepository, loggedOnUser, mapper);
			var existingOvertimeAvailability = MockRepository.GenerateMock<IOvertimeAvailability>();
			overtimeAvailabilityRepository.Stub(x => x.Find(input.Date, person))
										  .Return(new List<IOvertimeAvailability>
				                              {
					                              existingOvertimeAvailability
				                              });
			var viewModel = new OvertimeAvailabilityViewModel();
			mapper.Stub(x => x.Map<IOvertimeAvailability, OvertimeAvailabilityViewModel>(existingOvertimeAvailability))
				  .Return(viewModel);

			var result = target.Persist(input);
			result.Should().Be.SameInstanceAs(viewModel);

		}
	}
	[TestFixture]
	public class PreferencePersisterTest
	{
		[Test]
		public void ShouldAddPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper,  mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());
			var input = new PreferenceDayInput();

			mapper.Stub(x => x.Map<PreferenceDayInput, IPreferenceDay>(input)).Return(preferenceDay);

			target.Persist(input);

			preferenceDayRepository.AssertWasCalled(x => x.Add(preferenceDay));
			mapper.AssertWasNotCalled(x => x.Map<PreferenceDayInput, IPreferenceDay>(input, (IPreferenceDay)null));
		}

		[Test]
		public void ShouldReturnInputResultModelOnAdd()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(MockRepository.GenerateMock<IPreferenceDayRepository>(), mapper, mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());
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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, loggedOnUser);

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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, MockRepository.GenerateMock<IMappingEngine>(), mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());

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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());
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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, null, mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.FindAndLock(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay1, preferenceDay2 });

			target.Delete(DateOnly.Today);

			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay1));
			preferenceDayRepository.AssertWasCalled(x => x.Remove(preferenceDay2));
		}

		[Test]
		public void ShouldReturnEmptyInputResultOnDelete()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, null, mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.FindAndLock(DateOnly.Today, null)).Return(new List<IPreferenceDay> { preferenceDay });

			var result = target.Delete(DateOnly.Today);

			result.Preference.Should().Be.Null();
		}

		[Test]
		public void ShouldThrowHttp404OIfPreferenceDoesNotExists()
		{
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, null, mustHaveRestrictionSetter, MockRepository.GenerateMock<ILoggedOnUser>());

			preferenceDayRepository.Stub(x => x.FindAndLock(DateOnly.Today, null)).Return(new List<IPreferenceDay>());

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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, loggedOnUser);
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
		public void ShouldClearMustHavesWhenAppyNewPreference()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var preferenceDay = MockRepository.GenerateMock<IPreferenceDay>();
			var input = new PreferenceDayInput { Date = DateOnly.Today };
			var person = new Person();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, loggedOnUser);
			var preferenceRestriction = new PreferenceRestriction
			                            	{
				MustHave = true
			};
			preferenceDay.Stub(x => x.Restriction).Return(preferenceRestriction);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			preferenceDay.Restriction.MustHave.Should().Be.False();
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
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, loggedOnUser);

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			preferenceDayRepository.Stub(x => x.Find(input.Date, person)).Return(new List<IPreferenceDay> { preferenceDay });
			mapper.Stub(x => x.Map(input, preferenceDay)).Return(preferenceDay);

			target.Persist(input);

			preferenceDay.TemplateName.Should().Be.Null();
		}
		
		[Test]
		public void ShouldSetMustHave()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceDayRepository = MockRepository.GenerateMock<IPreferenceDayRepository>();
			var person = MockRepository.GenerateMock<IPerson>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var mustHaveRestrictionSetter = MockRepository.GenerateMock<IMustHaveRestrictionSetter>();
			var target = new PreferencePersister(preferenceDayRepository, mapper, mustHaveRestrictionSetter, loggedOnUser);
			var input = new MustHaveInput { Date = DateOnly.Today, MustHave = true };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			target.MustHave(input);
			mustHaveRestrictionSetter.AssertWasCalled(x => x.SetMustHave(input.Date, person, input.MustHave));
		}
	}
}



