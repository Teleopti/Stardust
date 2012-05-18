using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	public class PreferenceDayFeedbackViewModelMappingTest
	{
		private IPreferenceFeedbackProvider preferenceFeedbackProvider;

		[SetUp]
		public void Setup()
		{
			preferenceFeedbackProvider = MockRepository.GenerateMock<IPreferenceFeedbackProvider>();

			Mapper.Reset();
			Mapper.Initialize(x => x.AddProfile(new PreferenceDayFeedbackViewModelMappingProfile(Depend.On(preferenceFeedbackProvider))));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapDate()
		{
			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.Date.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			                     	{
			                     		StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			                     	};
			PreferenceType? preferenceType;
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today, out preferenceType)).Return(workTimeMinMax);

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleStartTimes.Should().Be(workTimeMinMax.StartTimeLimitation.StartTimeString + "-" + workTimeMinMax.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			                     	{
			                     		EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(19))
			                     	};

			PreferenceType? preferenceType;
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today, out preferenceType)).Return(workTimeMinMax);

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleEndTimes.Should().Be(workTimeMinMax.EndTimeLimitation.StartTimeString + "-" + workTimeMinMax.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleContractTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			                     	{
			                     		WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			                     	};
			PreferenceType? preferenceType;
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today, out preferenceType)).Return(workTimeMinMax);

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleContractTimes.Should().Be(workTimeMinMax.WorkTimeLimitation.StartTimeString + "-" + workTimeMinMax.WorkTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			PreferenceType? preferenceType;
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today, out preferenceType)).Return(null);

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}

	}
}