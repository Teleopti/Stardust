﻿using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDomainDataMappingTest
	{
		private IVirtualSchedulePeriodProvider virtualScheduleProvider;
		private IPreferenceProvider preferenceProvider;
		private IPerson person;
		private IPreferenceFeedbackProvider _preferenceFeedbackProvider;

		[SetUp]
		public void Setup()
		{
			virtualScheduleProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			_preferenceFeedbackProvider = MockRepository.GenerateMock<IPreferenceFeedbackProvider>();

			person = new Person
			         	{
			         		WorkflowControlSet = new WorkflowControlSet(null)
			         		                     	{
			         		                     		PreferencePeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1)),
			         		                     		PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
			         		                     	}
			         	};
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(
				new PreferenceDomainDataMappingProfile(
					Resolver.Of(() => virtualScheduleProvider),
					Resolver.Of(() => preferenceProvider),
					Resolver.Of(() => loggedOnUser),
					Resolver.Of(() => _preferenceFeedbackProvider)
					)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapSelectedDate()
		{
			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.SelectedDate.Should().Be.EqualTo(DateOnly.Today);
		}

		[Test]
		public void ShouldMapPeriod()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Period.Should().Be(period);
		}

		[Test]
		public void ShouldMapWorkflowControlSet()
		{
			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.WorkflowControlSet.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}





		[Test]
		public void ShouldMapDays()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(3));

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			var expected = DateOnly.Today.DateRange(4);
			result.Days.Select(d => d.Date).Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldMapPreferenceDay()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var preferenceDay = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			preferenceProvider.Stub(x => x.GetPreferencesForPeriod(period)).Return(new[] { preferenceDay });

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Days.Single().PreferenceDay.Should().Be.SameInstanceAs(preferenceDay);
		}

		[Test]
		public void ShouldMapWorkTimeMinMax()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var workTimeMinMax = new WorkTimeMinMax();

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			_preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(workTimeMinMax);

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Days.Single().WorkTimeMinMax.Should().Be(workTimeMinMax);
		}
	}
}