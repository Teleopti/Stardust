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
			preferenceFeedbackProvider.Stub(x => x.CheckNightRestViolation(DateOnly.Today)).Return(new PreferenceNightRestCheckResult());

			Mapper.Reset();
			Mapper.Initialize(x => x.AddProfile(new PreferenceDayFeedbackViewModelMappingProfile(preferenceFeedbackProvider, new Lazy<IMappingEngine>(() => Mapper.Engine))));
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
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

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

			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(new WorkTimeMinMaxCalculationResult { WorkTimeMinMax = workTimeMinMax });

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleEndTimes.Should().Be(workTimeMinMax.EndTimeLimitation.StartTimeString + "-" + workTimeMinMax.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesLower()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(new WorkTimeMinMaxCalculationResult { WorkTimeMinMax = workTimeMinMax });

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleContractTimeMinutesLower.Should().Be(workTimeMinMax.WorkTimeLimitation.StartTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesUpper()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(new WorkTimeMinMaxCalculationResult { WorkTimeMinMax = workTimeMinMax });

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.PossibleContractTimeMinutesUpper.Should().Be(workTimeMinMax.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(null);

			var result = Mapper.Map<DateOnly, PreferenceDayFeedbackViewModel>(DateOnly.Today);

			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}

	}
}