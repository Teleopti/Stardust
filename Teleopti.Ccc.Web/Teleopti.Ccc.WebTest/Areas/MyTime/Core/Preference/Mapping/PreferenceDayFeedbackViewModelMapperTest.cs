using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	[SetCulture("sv-SE")]
	public class PreferenceDayFeedbackViewModelMapperTest
	{
		[SetUp]
		public void Setup()
		{
			var today = DateOnly.Today;
			preferenceFeedbackProvider = MockRepository.GenerateMock<IPreferenceFeedbackProvider>();
			preferenceFeedbackProvider.Stub(x => x.CheckNightRestViolation(today))
				.Return(new PreferenceNightRestCheckResult());

			var perriodNightRestCheckResult =
				new Dictionary<DateOnly, PreferenceNightRestCheckResult>
				{
					{today, new PreferenceNightRestCheckResult()}
				};
			preferenceFeedbackProvider.Stub(x => x.CheckNightRestViolation(new DateOnlyPeriod(today, today)))
				.Return(perriodNightRestCheckResult);
			var toggleManager = new TrueToggleManager();

			target = new PreferenceDayFeedbackViewModelMapper(preferenceFeedbackProvider, toggleManager);
		}

		private IPreferenceFeedbackProvider preferenceFeedbackProvider;
		private PreferenceDayFeedbackViewModelMapper target;

		[Test]
		public void ShouldMapDate()
		{
			var result = target.Map(DateOnly.Today);
			result.Date.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesLower()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			result.PossibleContractTimeMinutesLower.Should()
				.Be(workTimeMinMax.WorkTimeLimitation.StartTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesUpper()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			result.PossibleContractTimeMinutesUpper.Should()
				.Be(workTimeMinMax.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(19))
			};

			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);
			result.PossibleEndTimes.Should()
				.Be(workTimeMinMax.EndTimeLimitation.StartTimeString + "-" + workTimeMinMax.EndTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			result.PossibleStartTimes.Should()
				.Be(workTimeMinMax.StartTimeLimitation.StartTimeString + "-" + workTimeMinMax.StartTimeLimitation.EndTimeString);
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			preferenceFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(null);

			var result = target.Map(DateOnly.Today);

			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}
	}
}