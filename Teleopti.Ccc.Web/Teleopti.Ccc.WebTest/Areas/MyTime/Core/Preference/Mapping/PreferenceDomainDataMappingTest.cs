using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Core;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDomainDataMappingTest
	{
		private IVirtualSchedulePeriodProvider virtualScheduleProvider;
		private IPreferenceProvider preferenceProvider;
		private IPerson person;
		private IPreferenceFeedbackProvider preferenceFeedbackProvider;
		private IScheduleProvider scheduleProvider;
		private IProjectionProvider projectionProvider;

		[SetUp]
		public void Setup()
		{
			virtualScheduleProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			preferenceFeedbackProvider = MockRepository.GenerateMock<IPreferenceFeedbackProvider>();
			scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
 
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
					Depend.On(virtualScheduleProvider),
					Depend.On(preferenceProvider),
					Depend.On(loggedOnUser),
					Depend.On(preferenceFeedbackProvider),
					Depend.On(scheduleProvider),
					Depend.On(projectionProvider)
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
		public void ShouldMapScheduleDay()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] {scheduleDay});

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Days.Single().ScheduleDay.Should().Be(scheduleDay);
		}

		[Test]
		public void ShouldMapProjection()
		{
			var stubs = new StubFactory();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);
			var projection = stubs.ProjectionStub();

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Days.Single().Projection.Should().Be.SameInstanceAs(projection);
		}

		[Test]
		public void ShouldNotMapScheduleDayAndProjectionIfNotScheduled()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });

			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.Days.Single().ScheduleDay.Should().Be.Null();
			result.Days.Single().Projection.Should().Be.Null();
		}

		[Test]
		public void ShouldMapColorSource()
		{
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			scheduleDay.Stub(x => x.IsScheduled()).Return(true);
			var projection = new StubFactory().ProjectionStub();
			var preferenceDay = new PreferenceDay(null, DateOnly.Today, new PreferenceRestriction());

			virtualScheduleProvider.Stub(x => x.GetCurrentOrNextVirtualPeriodForDate(DateOnly.Today)).Return(period);
			scheduleProvider.Stub(x => x.GetScheduleForPeriod(period)).Return(new[] { scheduleDay });
			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);
			preferenceProvider.Stub(x => x.GetPreferencesForPeriod(period)).Return(new[] {preferenceDay});
			
			var result = Mapper.Map<DateOnly, PreferenceDomainData>(DateOnly.Today);

			result.ColorSource.ScheduleDays.Single().Should().Be(scheduleDay);
			result.ColorSource.Projections.Single().Should().Be.SameInstanceAs(projection);
			result.ColorSource.PreferenceDays.Single().Should().Be(preferenceDay);
			result.ColorSource.WorkflowControlSet.Should().Be(person.WorkflowControlSet);
		}

	}
}