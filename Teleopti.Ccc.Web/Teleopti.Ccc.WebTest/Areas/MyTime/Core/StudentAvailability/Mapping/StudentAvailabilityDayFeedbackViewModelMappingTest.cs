using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.StudentAvailability.Mapping
{
	[TestFixture, SetCulture("sv-SE")]
	public class StudentAvailabilityDayFeedbackViewModelMappingTest
	{
		private IStudentAvailabilityFeedbackProvider studentAvailabilityFeedbackProvider;
		private StudentAvailabilityDayFeedbackViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			studentAvailabilityFeedbackProvider = MockRepository.GenerateMock<IStudentAvailabilityFeedbackProvider>();

			target = new StudentAvailabilityDayFeedbackViewModelMapper(studentAvailabilityFeedbackProvider);
		}
		
		[Test]
		public void ShouldMapDate()
		{
			var result = target.Map(DateOnly.Today);

			result.Date.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			var expectedResult =
				$"{workTimeMinMax.StartTimeLimitation.StartTimeString}-{workTimeMinMax.StartTimeLimitation.EndTimeString}";
			result.PossibleStartTimes.Should().Be(expectedResult);
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(15), TimeSpan.FromHours(19))
			};

			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			var expectedResult =
				$"{workTimeMinMax.EndTimeLimitation.StartTimeString}-{workTimeMinMax.EndTimeLimitation.EndTimeString}";
			result.PossibleEndTimes.Should().Be(expectedResult);
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesLower()
		{
			var workTimeMinMax = new WorkTimeMinMax
			{
				WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(10))
			};
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
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
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today))
				.Return(new WorkTimeMinMaxCalculationResult {WorkTimeMinMax = workTimeMinMax});

			var result = target.Map(DateOnly.Today);

			result.PossibleContractTimeMinutesUpper.Should()
				.Be(workTimeMinMax.WorkTimeLimitation.EndTime.Value.TotalMinutes.ToString());
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			studentAvailabilityFeedbackProvider.Stub(x => x.WorkTimeMinMaxForDate(DateOnly.Today)).Return(null);
			var result = target.Map(DateOnly.Today);
			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}
	}
}