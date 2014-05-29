using System.Collections.Generic;
using System.Web;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.ViewModelFactory
{
	[TestFixture]
	public class PreferenceViewModelFactoryTest
	{

		[Test]
		public void ShoudCreateViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper, null, null, null, null);
			var domainData = new PreferenceDomainData();
			var viewModel = new PreferenceViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDomainData>(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<PreferenceDomainData, PreferenceViewModel>(domainData)).Return(viewModel);

			var result = target.CreateViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldCreateDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			var target = new PreferenceViewModelFactory(mapper, preferenceProvider, null, null, null);
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today, new PreferenceRestriction());
			var viewModel = new PreferenceDayViewModel();

			preferenceProvider.Stub(x => x.GetPreferencesForDate(DateOnly.Today)).Return(preferenceDay);
			mapper.Stub(x => x.Map<IPreferenceDay, PreferenceDayViewModel>(preferenceDay)).Return(viewModel);

			var result = target.CreateDayViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldThrow404IfPreferenceDoesNotExist()
		{
			var preferenceProvider = MockRepository.GenerateMock<IPreferenceProvider>();
			var target = new PreferenceViewModelFactory(null, preferenceProvider, null, null, null);

			preferenceProvider.Stub(x => x.GetPreferencesForDate(DateOnly.Today)).Return(null);

			target.CreateDayViewModel(DateOnly.Today).Should().Be.Null();
		}

		[Test]
		public void ShouldCreateFeedbackDayViewModelByMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PreferenceViewModelFactory(mapper, null, null, null, null);
			var viewModel = new PreferenceDayFeedbackViewModel();

			mapper.Stub(x => x.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today)).Return(viewModel);

			var result = target.CreateDayFeedbackViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void CreatePreferencesAndSchedulesViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var target = new PreferenceViewModelFactory(mapper, null, scheduleProvider, null, null);
			var scheduleDays = new IScheduleDay[] {};
			var preferenceDayViewModels = new PreferenceAndScheduleDayViewModel[] {};

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1))))
				.Return(scheduleDays);
			mapper.Stub(x => x.Map<IEnumerable<IScheduleDay>, IEnumerable<PreferenceAndScheduleDayViewModel>>(scheduleDays))
				.Return(preferenceDayViewModels);
			var result = target.CreatePreferencesAndSchedulesViewModel(DateOnly.Today, DateOnly.Today.AddDays(1));

			result.Should().Be.SameInstanceAs(preferenceDayViewModels);
		}

		[Test]
		public void CreatePreferenceTemplateViewModel()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var preferenceTemplatesProvider = MockRepository.GenerateMock<IPreferenceTemplateProvider>();
			var target = new PreferenceViewModelFactory(mapper, null, null, preferenceTemplatesProvider, null);
			var templates = new IExtendedPreferenceTemplate[] { };
			var preferenceTemplateViewModels = new PreferenceTemplateViewModel[] { };

			preferenceTemplatesProvider.Stub(x => x.RetrievePreferenceTemplates())
				.Return(templates);
			mapper.Stub(x => x.Map<IEnumerable<IExtendedPreferenceTemplate>, IEnumerable<PreferenceTemplateViewModel>>(templates))
				.Return(preferenceTemplateViewModels);
			var result = target.CreatePreferenceTemplateViewModels();

			result.Should().Be.SameInstanceAs(preferenceTemplateViewModels);
		}

		[Test]
		public void CreatePreferenceWeeklyWorkTimeViewModel()
		{
			 var weeklyWorkTimeProvider = MockRepository.GenerateMock<IPreferenceWeeklyWorkTimeSettingProvider>();
			 var target = new PreferenceViewModelFactory(null, null, null, null, weeklyWorkTimeProvider);
			 var preferenceWeeklyWorkTimeViewModels = new PreferenceWeeklyWorkTimeViewModel();
			 var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting();
			 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = 120;
			 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = 360;

			 weeklyWorkTimeProvider.Stub(x => x.RetrieveSetting(DateOnly.Today))
				 .Return(weeklyWorkTimeSetting);
			 var result = target.CreatePreferenceWeeklyWorkTimeViewModel(DateOnly.Today);

			 result.MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes);
			 result.MinWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes);
		}
	}

	
}
