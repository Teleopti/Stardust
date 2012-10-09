using System.Collections.ObjectModel;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceAndScheduleDayViewModelMappingTest
	{
		[SetUp]
		public void Setup()
		{
			Mapper.Reset();
			Mapper.Initialize(c =>
				{
					c.AddProfile(new PreferenceAndScheduleDayViewModelMappingProfile());
					c.AddProfile(new PreferenceDayViewModelMappingProfile(
						Depend.On(MockRepository.GenerateMock<IExtendedPreferencePredicate>()
						)));
					c.AddProfile(new PreferenceViewModelMappingProfile(
						             Depend.On<IScheduleColorProvider>(() => null),
						             Depend.On<IPreferenceFulfilledChecker>(() => null)
						             ));
					c.AddProfile(new CommonViewModelMappingProfile());
				});
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Date.Should().Be(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPreferenceViewModel()
		{
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today);
			var preferenceRestriction = new PreferenceRestriction();
			preferenceRestriction.DayOffTemplate = new DayOffTemplate(new Description("DO"));
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, preferenceRestriction);
			var personRestrictionCollection =
				new ReadOnlyObservableCollection<IScheduleData>(new ObservableCollection<IScheduleData>(new[] {preferenceDay}));

			scheduleDay.Stub(x => x.PersonRestrictionCollection()).Return(personRestrictionCollection);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Preference.Should().Not.Be.Null();
			result.Preference.Preference.Should().Be("DO");
		}

		[Test]
		public void ShouldMapDayOffViewModel()
		{
			var dayOff = new PersonDayOff(new Person(), new Scenario(" "), new DayOffTemplate(new Description("DO")), DateOnly.Today);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, dayOff);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.DayOff.Should().Not.Be.Null();
			result.DayOff.DayOff.Should().Be("DO");
		}

		[Test]
		public void ShouldMapAbsenceViewModel()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub("Illness");
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.FullDayAbsence, absence);

			var result = Mapper.Map<IScheduleDay, PreferenceAndScheduleDayViewModel>(scheduleDay);

			result.Absence.Should().Not.Be.Null();
			result.Absence.Absence.Should().Be("Illness");
		}


	}
}